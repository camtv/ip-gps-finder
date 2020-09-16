using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Helpers
{
	public class TimedCache<T>
	{
		object oLock = new object();

		class CacheEntry
		{
			public T Data;
			public CacheIdxEntry IdxPointer=null;
		}
		Dictionary<String, CacheEntry> kvCache = new Dictionary<String, CacheEntry>();

		class CacheIdxEntry
		{
			public String Key;
			public int Timestamp;
		}
		List<CacheIdxEntry> lsCacheIdx = new List<CacheIdxEntry>();

		int CACHE_ITEMS_MAX_COUNT = 10000;
		int CACHE_ITEMS_TTL_SECONDS = 60 * 24 * 30;

		public TimedCache(int nMaxEntries, int EntryTTLSeconds)
		{
			CACHE_ITEMS_MAX_COUNT = nMaxEntries;
			CACHE_ITEMS_TTL_SECONDS = EntryTTLSeconds;
			kvCache = new Dictionary<String, CacheEntry>(CACHE_ITEMS_MAX_COUNT);
			lsCacheIdx = new List<CacheIdxEntry>(CACHE_ITEMS_MAX_COUNT);
		}

		public Int32 GetUnixTimestamp()
		{
			Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

			return unixTimestamp;
		}

		void UpdateIndex()
		{
			int nNow = GetUnixTimestamp();
			while (lsCacheIdx.Count > 0 && (lsCacheIdx.Count > CACHE_ITEMS_MAX_COUNT || (nNow - lsCacheIdx[0].Timestamp) > CACHE_ITEMS_TTL_SECONDS))
			{
				String sk = lsCacheIdx[0].Key;
				lsCacheIdx.RemoveAt(0);
				try {
					if (kvCache.ContainsKey(sk) == true)
						kvCache.Remove(sk);
				} catch { };
			}
		}

		public Int64 IndexDepth {  get { return lsCacheIdx.Count;  } }
		public Int64 EntriesCount { get { return kvCache.Count; } }

		public T Get(String EntryKey)
		{
			T data = default(T);
			int nNow = GetUnixTimestamp();
			lock (oLock)
			{
				if (kvCache.ContainsKey(EntryKey) == true && (nNow - kvCache[EntryKey].IdxPointer.Timestamp) <= CACHE_ITEMS_TTL_SECONDS)
				{
					data = kvCache[EntryKey].Data;
					nNow = GetUnixTimestamp();
					CacheIdxEntry idxold = kvCache[EntryKey].IdxPointer;
					CacheIdxEntry idxnew = new CacheIdxEntry() { Key = EntryKey, Timestamp = nNow };
					kvCache[EntryKey].IdxPointer = idxnew;
					lsCacheIdx.Remove(idxold);
					lsCacheIdx.Add(idxnew);
				}
				UpdateIndex();
			}

			return data;
		}

		public T GetAndRemove(String EntryKey)
		{
			T data = default(T);
			int nNow = GetUnixTimestamp();
			lock (oLock)
			{
				if (kvCache.ContainsKey(EntryKey) == true && (nNow - kvCache[EntryKey].IdxPointer.Timestamp) <= CACHE_ITEMS_TTL_SECONDS)
				{
					data = kvCache[EntryKey].Data;
					nNow = GetUnixTimestamp();
					CacheIdxEntry idxold = kvCache[EntryKey].IdxPointer;
					lsCacheIdx.Remove(idxold);
					kvCache.Remove(EntryKey);
				}
				UpdateIndex();
			}

			return data;
		}

		public void Add(String EntryKey, T EntryData)
		{
			lock (oLock)
			{
				int nNow = GetUnixTimestamp();
				CacheIdxEntry idxnew = new CacheIdxEntry() { Key = EntryKey, Timestamp = nNow };

				if (kvCache.ContainsKey(EntryKey) == true)
				{
					CacheIdxEntry idxold = kvCache[EntryKey].IdxPointer;
					lsCacheIdx.Remove(idxold);
					kvCache[EntryKey].IdxPointer = idxnew;
					kvCache[EntryKey].Data = EntryData;
				}
				else
					kvCache[EntryKey] = new CacheEntry() { Data = EntryData, IdxPointer = idxnew };

				lsCacheIdx.Add(idxnew);
				UpdateIndex();
			}
		}

		public void Clear()
		{
			lock (oLock)
			{
				kvCache.Clear();
				lsCacheIdx.Clear();
			}
		}

		public class Tester
		{
			public static object Taks { get; private set; }

			static String SHA256(String data)
			{
				System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
				string hash = String.Empty;
				byte[] vb = System.Text.Encoding.UTF8.GetBytes(data);
				byte[] crypto = crypt.ComputeHash(vb);
				foreach (byte bit in crypto)
				{
					hash += bit.ToString("x2");
				}
				return hash;
			}

			static void Assert(bool Condition, String ErrorDesc = "")
			{
				if (Condition == false)
					throw new Exception("Assertion failed: " + ErrorDesc);
			}

			static int[] Shuffle(int[] list)
			{
				Random rnd = new Random(Environment.TickCount);
				for (int i = 0; i < list.Length - 1; i++)
				{
					int j = rnd.Next(i, list.Length);
					var temp = list[i];
					list[i] = list[j];
					list[j] = temp;
				}

				return list;
			}

			static List<int> CreateShuffledArray(int nEntries)
			{
				int[] vals = new int[nEntries];
				for (int i = 0; i < nEntries; i++)
					vals[i] = i;
				return new List<int>(Shuffle(vals));
			}

			public static String Perform(int nEntries, int DataLen, int TTLSeconds)
			{
				try
				{
					TimedCache<String> tc = new TimedCache<string>(nEntries, TTLSeconds);

					Console.WriteLine("Adding {0} entries ..", nEntries);
					Int64 ts = Environment.TickCount;
					for (int i = 0; i < nEntries; i++)
					{
						String val = i.ToString().PadLeft(DataLen, '0');
						tc.Add(SHA256(i.ToString()), val);
					}

					// Verifca che ci siano nEntries negli array
					Assert(tc.IndexDepth == nEntries, String.Format("Expected IndexDepth={0} but IndexDepth={1}", nEntries, tc.IndexDepth));
					Assert(tc.EntriesCount == nEntries, String.Format("Expected EntriesCount={0} but EntriesCount={1}", nEntries, tc.EntriesCount));
					Console.WriteLine("Done in {0} ms", Environment.TickCount - ts);

					// Verifica che tutte le entries ci siano
					Console.WriteLine("Verifing presence of entries in casual order..");
					List<int> ls = CreateShuffledArray(nEntries);
					ts = Environment.TickCount;
					while (ls.Count > 0)
					{
						int i = ls[0];
						ls.RemoveAt(0);
						String val = i.ToString().PadLeft(DataLen, '0');
						Assert(tc.Get(SHA256(i.ToString())) == val, String.Format("tc.Get(SHA256(i.ToString())!=i.ToString() for i={0}", i));
					}
					Assert(tc.Get(SHA256((nEntries + 1).ToString())) == null, String.Format("tc.Get(SHA256((nEntries+1).ToString())!=null for nEntries={0}", nEntries));


					// Verifca che ci siano nEntries negli array
					Assert(tc.IndexDepth == nEntries, String.Format("Expected IndexDepth={0} but IndexDepth={1}", nEntries, tc.IndexDepth));
					Assert(tc.EntriesCount == nEntries, String.Format("Expected EntriesCount={0} but EntriesCount={1}", nEntries, tc.EntriesCount));
					Console.WriteLine("Done in {0} ms", Environment.TickCount - ts);

					// Prova a sostituire le entries con valori casuali								
					Console.WriteLine("Replacing all entries data with casual values");
					List<int> ls1 = CreateShuffledArray(nEntries);
					ts = Environment.TickCount;
					for (int i = 0; i < nEntries; i++)
					{
						String val = ls1[i].ToString().PadLeft(DataLen, '0');
						tc.Add(SHA256(i.ToString()), val);
					}
					Assert(tc.IndexDepth == nEntries, String.Format("Expected IndexDepth={0} but IndexDepth={1}", nEntries, tc.IndexDepth));
					Assert(tc.EntriesCount == nEntries, String.Format("Expected EntriesCount={0} but EntriesCount={1}", nEntries, tc.EntriesCount));
					Console.WriteLine("Done in {0} ms", Environment.TickCount - ts);

					Console.WriteLine("Verifing presence of entries in casual order..");
					ls = CreateShuffledArray(nEntries);
					ts = Environment.TickCount;
					while (ls.Count > 0)
					{
						int i = ls[0];
						ls.RemoveAt(0);
						String s = tc.Get(SHA256(i.ToString()));
						String val = ls1[i].ToString().PadLeft(DataLen, '0');
						Assert(s == val, String.Format("tc.Get(SHA256(i.ToString())!=i.ToString() for i={0}", i));
					}
					Assert(tc.IndexDepth == nEntries, String.Format("Expected IndexDepth={0} but IndexDepth={1}", nEntries, tc.IndexDepth));
					Assert(tc.EntriesCount == nEntries, String.Format("Expected EntriesCount={0} but EntriesCount={1}", nEntries, tc.EntriesCount));
					Console.WriteLine("Done in {0} ms", Environment.TickCount - ts);

					Console.WriteLine("Performing 100.000 Get Of Cached entries..");
					List<int> lse = new List<int>();
					Stopwatch stopWatch = new Stopwatch();
					Random rnd = new Random(Environment.TickCount);
					stopWatch.Start();
					for (int i = 0; i < 100000; i++)
					{
						String s = tc.Get(SHA256(rnd.Next(nEntries).ToString()));
					}
					stopWatch.Stop();
					Console.WriteLine("Done in {0} ms", stopWatch.ElapsedMilliseconds);

					Console.WriteLine("Performing 100.000 Get Of NON Cached entries..");
					lse = new List<int>();
					stopWatch = new Stopwatch();
					rnd = new Random(Environment.TickCount);
					stopWatch.Start();
					for (int i = 0; i < 100000; i++)
					{
						String s = tc.Get(SHA256(rnd.Next(nEntries + 1, 2 * nEntries).ToString()));
					}
					stopWatch.Stop();
					Console.WriteLine("Done in {0} ms", stopWatch.ElapsedMilliseconds);

					Console.WriteLine("Sleeping over TTL time: {0}+1 seconds and then verify all cache empty..", TTLSeconds);
					Task.Delay(TTLSeconds * 1000 + 1000).Wait();
					tc.Get(SHA256(rnd.Next(nEntries).ToString()));
					Assert(tc.IndexDepth == 0, String.Format("Expected IndexDepth={0} but IndexDepth={1}", 1, tc.IndexDepth));
					Assert(tc.EntriesCount == 0, String.Format("Expected EntriesCount={0} but EntriesCount={1}", 1, tc.EntriesCount));
					Console.WriteLine("Done in {0} ms", Environment.TickCount - ts);


					return "";
				}
				catch (Exception Ex)
				{
					return Ex.Message;
				}
			}
		}
	}
}

