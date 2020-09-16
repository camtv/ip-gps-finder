using CSRedis;
using System;
using System.Threading.Tasks;

namespace Helpers
{
	public class SignalerBase
	{
		public static KeyValueHelperBase<TEventHandlerBase> kvEventHandlers = new KeyValueHelperBase<TEventHandlerBase>();

		public class TEvent
		{
			public String Name;
			public String Payload;
		}

		public delegate void TEventEmitter(String EventName, String Data);
		public static TEventEmitter EventEmitter = null;

		static void RegisterEventHandler(TEventHandlerBase evt)
		{
			kvEventHandlers[evt.EventName] = evt;
		}

		public class TEventHandlerBase
		{
			public String EventName;

			virtual public void TriggerHandler(String Data) { }
		}

		public class EventHandler<T> : TEventHandlerBase
		{
			public EventHandler(String sevn)
			{
				EventName = sevn;
				RegisterEventHandler(this);
			}

			public void Emit(T Data)
			{
				if (EventEmitter != null)
					EventEmitter(EventName, Newtonsoft.Json.JsonConvert.SerializeObject(Data));
			}

			public delegate void TEvtHandler(T Payload);
			TEvtHandler evtHandler = null;
			public void On(TEvtHandler eh)
			{
				evtHandler += eh;
			}

			override public void TriggerHandler(String Data)
			{
				try
				{
					T oData = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Data);
					if (evtHandler != null)
						evtHandler(oData);
				}
				catch (Exception Ex)
				{
					LogHelper.Error(Ex);
				}
			}
		}
	}

	public class RedisSignaler : SignalerBase
	{
		static RedisClient Redis;
		static object SyncObj = new object();

		static String INTERFACE_CHANNEL_NAME;
		static String REDIS_IP_ADDRESS;
		static int REDIS_PORT;

		static public bool Init(String RedisIPAddress, int RedisPort, String ChannelName)
		{
			lock (SyncObj)
			{
				try
				{
					INTERFACE_CHANNEL_NAME = ChannelName;
					REDIS_IP_ADDRESS = RedisIPAddress;
					REDIS_PORT = RedisPort;
					Redis = new RedisClient(REDIS_IP_ADDRESS, REDIS_PORT);
					Redis.ReconnectAttempts = 15;
					Redis.ReconnectWait = 200;

					SignalerBase.EventEmitter = EmitEvent;

					InitSubscribeLoop();

					return true;
				}
				catch (Exception Ex)
				{
					LogHelper.Error("Redis = new RedisClient - Exception: " + Ex.Message);
				}
			}

			return false;
		}

		static void InitSubscribeLoop()
		{
			RedisClient Redis = null;

			Task.Run(() =>
			{
				while (true)
				{
					try
					{
						if (Redis == null)
						{
							Redis = new RedisClient(REDIS_IP_ADDRESS, REDIS_PORT);
							Redis.ReconnectAttempts = 15;
							Redis.ReconnectWait = 200;
						}

						if (Redis.IsConnected == false)
						{
							if (Redis.Connect(5000) == false)
								throw new Exception("Redis.Connect(5000) == false");
						}

						Redis.SubscriptionReceived += (object sender, RedisSubscriptionReceivedEventArgs e) =>
						{
							try
							{
								if (e.Message.Channel != INTERFACE_CHANNEL_NAME)
									return;

								TEvent evt = Newtonsoft.Json.JsonConvert.DeserializeObject<TEvent>(e.Message.Body);

								TEventHandlerBase evtHandler = kvEventHandlers[evt.Name];
								if (evtHandler != null)
									evtHandler.TriggerHandler(evt.Payload);
							}
							catch (Exception Ex)
							{
								LogHelper.Error("Redis.SubscriptionReceived - Exception: " + Ex.Message);
							}
						};
						Redis.Subscribe(INTERFACE_CHANNEL_NAME);
					}
					catch (Exception Ex)
					{
						LogHelper.Error("Redis = new RedisClient - Exception: " + Ex.Message);
					}
					Redis = null;
					Task.Delay(1000).Wait();
				}
			});
		}

		static void EmitEvent(String EventName, String Data)
		{
			lock (SyncObj)
			{
				try
				{
					if (Redis.IsConnected == false)
					{
						if (Redis.Connect(5000) == false)
							throw new Exception("Redis.Connect(5000) == false");
					}

					Redis.Publish(INTERFACE_CHANNEL_NAME, Newtonsoft.Json.JsonConvert.SerializeObject(new TEvent() { Name = EventName, Payload = Data }));
				}
				catch (Exception Ex)
				{
					LogHelper.Error("Signal - Exception: " + Ex.Message);
				}
			}
		}
	}

}
