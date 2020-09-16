using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Helpers
{
	public class SKEY
	{
		static int RandPrefixLen = 2;

		public static String Encode(Int64 SessionID,Int64 UserID)
		{
			if (SessionID == -1 || SessionID == 0)
				return null;

			if (UserID == -1 || UserID == 0)
				return null;

			byte[] vbsid = BitConverter.GetBytes(SessionID);
			byte[] vbuid = BitConverter.GetBytes(UserID);

			MemoryStream ms = new MemoryStream();
			ms.Write(vbsid, 0, vbsid.Length);
			ms.Write(vbuid, 0, vbuid.Length);

			String s = Crypt.CryptBytes(ms.ToArray(), 1);

			return Crypt.CryptStr(s, RandPrefixLen);
		}

		public static bool Decode(String sKey, out Int64 SessionID, out Int64 UserID)
		{
			SessionID = -1;
			UserID = -1;

			if (sKey == null)
				return false;

			int buflen = 2 * BitConverter.GetBytes(SessionID).Length;

			String s = Crypt.UnCryptStr(sKey, RandPrefixLen);

			byte[] decbuf = Crypt.UnCryptBytes(s, 1);
			if (decbuf==null || decbuf.Length != buflen)
				return false;

			SessionID = BitConverter.ToInt64(decbuf, 0);
			UserID = BitConverter.ToInt64(decbuf, buflen/2);

			return ((SessionID > 0) && (UserID > 0));
		}
	}

	public class CHKEY
	{
		static int RandPrefixLen = 0;

		public static String Encode(Int64 UserID,Int64 ChannelID,Int64 RemoteUserID)
		{
			if (ChannelID == -1 || ChannelID == 0)
				return null;

			if (UserID == -1 || UserID == 0)
				return null;

			byte[] vbuid = BitConverter.GetBytes(UserID);
			byte[] vbchid = BitConverter.GetBytes(ChannelID);
			byte[] vbruid = BitConverter.GetBytes(RemoteUserID);

			MemoryStream ms = new MemoryStream();			
			ms.Write(vbuid, 0, vbuid.Length);
			ms.Write(vbchid, 0, vbchid.Length);
			ms.Write(vbruid, 0, vbruid.Length);

			String s = Crypt.CryptBytes(ms.ToArray(), 0);

			return Crypt.CryptStr(s, RandPrefixLen);
		}

		public static bool Decode(String sChKey, out Int64 UserID,out Int64 ChannelID,out Int64 RemoteUserID)
		{
			ChannelID = -1;
			UserID = -1;
			RemoteUserID = 0;

			if (sChKey == null)
				return false;

			int idLen = BitConverter.GetBytes(ChannelID).Length;
			int buflen = 3 * idLen;

			String s = Crypt.UnCryptStr(sChKey, RandPrefixLen);

			byte[] decbuf = Crypt.UnCryptBytes(s, 0);
			if (decbuf == null || decbuf.Length != buflen)
				return false;
			
			UserID = BitConverter.ToInt64(decbuf, 0);
			ChannelID = BitConverter.ToInt64(decbuf, idLen);
			RemoteUserID = BitConverter.ToInt64(decbuf, idLen + idLen);

			return ((ChannelID > 0) && (UserID > 0) && (RemoteUserID>0));
		}
	}

	public class KeyEncDec
	{
		static int RandPrefixLen = 2;

		public static String Encode(Int64 UserID, byte[] data)
		{
			if (UserID == -1 || UserID == 0)
				return null;

			byte[] vbuid = BitConverter.GetBytes(UserID);

			MemoryStream ms = new MemoryStream();
			ms.Write(vbuid, 0, vbuid.Length);
			ms.Write(data, 0, data.Length);

			String s = Crypt.CryptBytes(ms.ToArray(), 1);

			return Crypt.CryptStr(s, RandPrefixLen);
		}

		public static bool Decode(String sEncKey, out Int64 UserID, out byte[] data)
		{
			UserID = -1;
			data = null;

			if (sEncKey == null)
				return false;

			String s = Crypt.UnCryptStr(sEncKey, RandPrefixLen);

			byte[] decbuf = Crypt.UnCryptBytes(s, 1);
			if (decbuf == null)
				return false;

			UserID = BitConverter.ToInt64(decbuf, 0);
			Buffer.BlockCopy(decbuf, sizeof(Int64), data, 0, decbuf.Length - sizeof(Int64));

			return true;
		}
	}
}
