using CSRedis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Helpers
{
	public abstract class CMapIDBase<T>
	{
		public delegate bool TComparer(T Value);
		public delegate bool TComparerKey(Int64 ID);
		public delegate bool TChecker(Int64 ID);
		public delegate void TModifier(ref T Value);

		public abstract void Set(Int64 ID, T Value);
		public abstract void Modify(Int64 ID, TModifier func);
		public abstract void SetRange(ICollection<Int64> vID, T Value, TChecker cmp = null);
		public abstract bool Contains(Int64 ID);
		public abstract T Get(Int64 ID, T EmptyValue);
		public abstract Int64[] GetRange(TComparer cmpr);
		public abstract void Remove(Int64 ID);
		public abstract void RemoveRange(ICollection<Int64> vID);
	}

	public class CMapIDToData<T> : CMapIDBase<T>
	{
		object LockObj = new object();
		Dictionary<Int64, T> dData = new Dictionary<Int64, T>();

		public override void Set(Int64 ID, T Value)
		{
			lock (LockObj)
			{
				dData[ID] = Value;
			}
		}

		public override void Modify(Int64 ID, TModifier func)
		{
			lock (LockObj)
			{
				if (dData.ContainsKey(ID))
				{
					T el = dData[ID];
					func(ref el);
				}
			}
		}

		public override void SetRange(ICollection<Int64> vID, T Value, TChecker cmp = null)
		{
			lock (LockObj)
			{
				foreach (Int64 ID in vID)
				{
					if (cmp != null && cmp(ID) == true)
						dData[ID] = Value;
				}
			}
		}

		public override bool Contains(Int64 ID)
		{
			lock (LockObj)
			{
				return dData.ContainsKey(ID);
			}
		}

		public override T Get(Int64 ID, T EmptyValue)
		{
			lock (LockObj)
			{
				if (dData.ContainsKey(ID))
					return dData[ID];
			}

			return EmptyValue;
		}

		public override Int64[] GetRange(TComparer cmpr)
		{
			List<Int64> ls = new List<Int64>();
			lock (LockObj)
			{
				foreach (Int64 ID in dData.Keys)
				{
					if (cmpr(dData[ID]) == true)
						ls.Add(ID);
				}
			}

			return ls.ToArray();
		}

		public override void Remove(Int64 ID)
		{
			lock (LockObj)
			{
				if (dData.ContainsKey(ID))
					dData.Remove(ID);
			}
		}

		public override void RemoveRange(ICollection<Int64> vID)
		{
			lock (LockObj)
			{
				foreach (Int64 ID in vID)
				{
					if (dData.ContainsKey(ID))
						dData.Remove(ID);
				}
			}
		}
	}

	public class CMapIDToRedis<T> : CMapIDBase<T>
	{
		object LockObj = new object();
		RedisClient Redis = null;
		int NDatabase = 10;

		public CMapIDToRedis(int NDatabase, String RedisIP, int RedisPort)
		{
			lock (LockObj)
			{
				try
				{
					this.NDatabase = NDatabase;
					Redis = new RedisClient(RedisIP, RedisPort);
					Redis.Select(NDatabase);
					Redis.ReconnectAttempts = 15;
					Redis.ReconnectWait = 200;
				}
				catch (Exception Ex)
				{
					throw new Exception("Redis = new RedisClient - Exception: " + Ex.Message);
				}
			}
		}

		public void EnsureRedisIsConnected()
		{
			if (Redis.IsConnected == false)
			{
				if (Redis.Connect(5000) == false)
					throw new Exception("Redis.Connect(5000) == false");
			}
		}

		String Prefix(Int64 ID)
		{
			return ID.ToString();
		}

		public override void Set(Int64 ID, T Value)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();

				Redis.Set(Prefix(ID), JsonConvert.SerializeObject(Value));
			}
		}

		public override void Modify(Int64 ID, TModifier func)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();

				String val = Redis.Get(Prefix(ID));
				if (val != null)
				{
					T el = JsonConvert.DeserializeObject<T>(val);
					func(ref el);
					Redis.Set(Prefix(ID), JsonConvert.SerializeObject(el));
				}
			}
		}

		public override void SetRange(ICollection<Int64> vID, T Value, TChecker cmp = null)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				foreach (Int64 ID in vID)
				{
					if (cmp != null && cmp(ID) == true)
					{
						Redis.Set(Prefix(ID), JsonConvert.SerializeObject(Value));
					}
				}
			}
		}

		public override bool Contains(Int64 ID)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				return Redis.Exists(Prefix(ID));
			}
		}

		public override T Get(Int64 ID, T EmptyValue)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				String val = Redis.Get(Prefix(ID));
				if (val != null)
					return JsonConvert.DeserializeObject<T>(val);
			}

			return EmptyValue;
		}

		public override Int64[] GetRange(TComparer cmpr)
		{
			List<Int64> ls = new List<Int64>();
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				String[] vSID = Redis.Keys("*");
				foreach (String sID in vSID)
				{
					String val = Redis.Get(sID);
					T el = JsonConvert.DeserializeObject<T>(val);
					if (cmpr(el) == true)
						ls.Add(Types.ToInt64(sID));
				}
			}

			return ls.ToArray();
		}

		public Int64[] GetRangeKeys(TComparerKey cmpr)
		{
			List<Int64> ls = new List<Int64>();
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				String[] vSID = Redis.Keys("*");
				foreach (String sID in vSID)
				{
					if (cmpr(Types.ToInt64(sID)) == true)
						ls.Add(Types.ToInt64(sID));
				}
			}

			return ls.ToArray();
		}

		public override void Remove(Int64 ID)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				Redis.Del(new String[] { Prefix(ID) });
			}
		}

		public override void RemoveRange(ICollection<Int64> vID)
		{
			lock (LockObj)
			{
				EnsureRedisIsConnected();
				List<String> vSID = new List<String>();
				foreach (Int64 id in vID)
					vSID.Add(Prefix(id));
				Redis.Del(vSID.ToArray());
			}
		}
	}
}
