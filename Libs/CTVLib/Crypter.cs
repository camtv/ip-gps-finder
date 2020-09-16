using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
	public class Crypt
	{
		public static String CryptStr(String str, int RandomPrefixLen = CCrypter.DEFAULT_RND_PREFIX)
		{
			CCrypter cr = new CCrypter(RandomPrefixLen);
			byte[] bytes = Encoding.UTF8.GetBytes(str);
				
			return cr.En(bytes);
		}

		public static String UnCryptStr(String encrstr, int RandomPrefixLen = CCrypter.DEFAULT_RND_PREFIX)
		{
			CCrypter cr = new CCrypter(RandomPrefixLen);
			byte[] bytes = cr.Un(encrstr);
			if (bytes == null)
				return "";
			return Encoding.UTF8.GetString(bytes);
		}

		public static String CryptBytes(byte[] bytes, int RandomPrefixLen = CCrypter.DEFAULT_RND_PREFIX)
		{
			CCrypter cr = new CCrypter(RandomPrefixLen);
			return cr.En(bytes);
		}

		public static byte[] UnCryptBytes(String encrstr, int RandomPrefixLen = CCrypter.DEFAULT_RND_PREFIX)
		{
			CCrypter cr = new CCrypter(RandomPrefixLen);
			return cr.Un(encrstr);
		}

		public static bool SetCryptTable(String CryptTable) { return CCrypter.SetCryptTable(CryptTable); }
		public static bool Test() { return CCrypter.Test(); }
	}

	class CCrypter
	{			   
		public const int DEFAULT_RND_PREFIX = 8;

		public static List<String> CryptTableElements = null;

		private int nPrefix = DEFAULT_RND_PREFIX;    
	
		public CCrypter(int _nPrefix=DEFAULT_RND_PREFIX) 
		{
			nPrefix = _nPrefix;
		}

		public String En(byte[] bytes)
		{
			String bufc = "";

			byte[] buf = new byte[bytes.Length+nPrefix+1];
			byte[] rand = GenerateRandomBytes(nPrefix);
			byte xor = 0;
			int ibr = 0;
			int ibb = 0;
			for (int i = 1; i < bytes.Length + nPrefix + 1;i++)
			{
				if (i <= rand.Length)
					buf[i] = rand[ibr++];
				else
					buf[i] = bytes[ibb++];

				xor ^= buf[i];
			}
			buf[0] = xor;

			if (!Encrypt(buf, ref bufc))
				return "";
		
			return bufc;
		}

		public byte[] Un(String encrstr)
		{
			byte[] bytes;

			if (!UnEncrypt(out bytes, encrstr))
				return null;

			if (bytes.Length < nPrefix)
				return null;

			byte xor = 0;

			for (int i = 1; i < bytes.Length; i++)
				xor ^= bytes[i];

			if (xor != bytes[0])
				return null;

			byte[] RetBytes = new byte[bytes.Length - nPrefix-1];
			System.Buffer.BlockCopy(bytes, nPrefix+1, RetBytes, 0, RetBytes.Length);

			return RetBytes;
		}

		private bool Encrypt(byte[] bytes, ref String sEncrypted)
		{
			if (CryptTableElements == null)
				return false;

			int nOffset = 0;
			sEncrypted = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				int nChar = (nOffset + ((int)bytes[i]));
				if (nChar >= 256)
					nChar -= 256;
				nOffset = nChar;
				sEncrypted += CryptTableElements[nChar][0];
				sEncrypted += CryptTableElements[nChar][1];
			}
			return true;
		}
		
		// @desc Decrypts the string passed in and saves to sTxt
		private bool UnEncrypt(out byte[] bytes,String sEncrypted)
		{
			bytes = null;

			if (CryptTableElements == null)
				return false;

			int n = 0;
			int i = 0;
			int sCLen = sEncrypted.Length;
			String s2ch = "";
			int nOffset = 0;
					
			// the length of the encrypted string must be even
			if (sCLen % 2 != 0)
				return false;

			bytes = new byte[sCLen / 2];
			int nBytes = 0;

			while (n < sCLen)
			{
				s2ch = "";
				s2ch += sEncrypted[n++];
				s2ch += sEncrypted[n++];
			
				for (i=0;i<256;i++)
				{
					if (s2ch == CryptTableElements[i])
					{
						int nChar = i - nOffset;
						if (nChar < 0)
							nChar = 256 + i - nOffset;
						nOffset = i;
						bytes[nBytes] = (byte)(nChar);
						nBytes++;
						break;
					}
				}

				if (i >= 256)
					return false;
			}

			return true;
		}
		
		public byte[] GenerateRandomBytes(int nLen)
		{
			Random random = new Random();
			int randomNumber = random.Next(0, 100);

			byte[] ret = new byte[nLen];
			random.NextBytes(ret);
			return ret;
		}

		public String GetRndString(int Len)
		{
			String bufc = "";

			if (!Encrypt(GenerateRandomBytes(Len / 2), ref bufc))
				return "";
		
			return bufc;       
		}

		public static bool SetCryptTable(String sCryptTable)
		{
			CryptTableElements = null;

			if (sCryptTable.Length != 256 * 2)
				return false;

			CryptTableElements = new List<string>();

			for (int i = 0; i < 256; i++)
			{
				String sEl = sCryptTable.Substring(2*i,2);
				if (CryptTableElements.IndexOf(sEl) != -1)
				{
					CryptTableElements = null;
					return false;
				}
				CryptTableElements.Add(sEl);
			}

			return true;
		}

		public static bool Test()
		{
			String[]  vs = new String[]
			{
				"12 Amazing and Essential Linux Books To Enrich Your Brain and Library"		,
				"50 UNIX / Linux Sysadmin Tutorials"										,
				"50 Most Frequently Used UNIX / Linux Commands (With Examples)"				,
				"How To Be Productive and Get Things Done Using GTD"						,
				"30 Things To Do When you are Bored and have a Computer"					,
				"Linux Directory Structure (File System Structure) Explained with Examples"	,
				"Linux Crontab: 15 Awesome Cron Job Examples"								,
				"Get a Grip on the Grep! – 15 Practical Grep Command Examples"				,
				"Unix LS Command: 15 Practical Examples"									,
				"15 Examples To Master Linux Command Line History"							,
				"Top 10 Open Source Bug Tracking System"									,
				"Vi and Vim Macro Tutorial: How To Record and Play"							,
				"Mommy, I found it! -- 15 Practical Linux Find Command Examples"			,
				"15 Awesome Gmail Tips and Tricks"											,
				"15 Awesome Google Search Tips and Tricks"									,
				"RAID 0, RAID 1, RAID 5, RAID 10 Explained with Diagrams"					,
				"Can You Top This? 15 Practical Linux Top Command Examples"					,
				"Top 5 Best System Monitoring Tools"										,
				"Top 5 Best Linux OS Distributions"											,
				"How To Monitor Remote Linux Host using Nagios 3.0"							,
				"Awk Introduction Tutorial – 7 Awk Print Examples"							,
				"How to Backup Linux? 15 rsync Command Examples"							,
				"The Ultimate Wget Download Guide With 15 Awesome Examples"					,
				"Top 5 Best Linux Text Editors"												,
			};

			for (int i = 1; i < 20; i++)
			{
				String sEncBuf;
				CCrypter enc1 = new CCrypter(i);
				CCrypter enc2 = new CCrypter(i);

				foreach (String s in vs)
				{
					sEncBuf = enc1.En(Encoding.UTF8.GetBytes(s));
					if (sEncBuf == "")
						return false;

					byte[] buf = enc2.Un(sEncBuf);
					if (buf == null)
						return false;
					String sd = Encoding.UTF8.GetString(buf);
					if (sd != s)
						return false;
				}
			}

			return true;
		}

	}


}


 