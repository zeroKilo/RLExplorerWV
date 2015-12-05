using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace EngineClasses
{
    public static class Helpers
    {
        public static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null || b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public static void WriteInt(Stream s, int i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteLEInt(Stream s, int i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static void WriteUInt(Stream s, uint i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteLEUInt(Stream s, uint i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static void WriteLEUShort(Stream s, ushort u)
        {
            byte[] buff = BitConverter.GetBytes(u);
            buff = buff.Reverse().ToArray();
            s.Write(buff, 0, 2);
        }

        public static int ReadInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        public static long ReadLong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToInt64(buff, 0);
        }

        public static ulong ReadULong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToUInt64(buff, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToSingle(buff, 0);
        }

        public static int ReadLEInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadLEUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadLEShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadLEUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt16(buff, 0);
        }

        public static byte[] ReadFull(Stream s, uint size)
        {
            byte[] buff = new byte[size];
            int totalread = 0;
            while ((totalread += s.Read(buff, totalread, (int)(size - totalread))) < size) ;
            return buff;
        }

        public static string ReadNullString(Stream s)
        {
            string res = "";
            byte b;
            while ((b = (byte)s.ReadByte()) > 0 && s.Position < s.Length) res += (char)b;
            return res;
        }

        public static void WriteNullString(Stream s, string t)
        {
            foreach (char c in t)
                s.WriteByte((byte)c);
            s.WriteByte(0);
        }

        public static ulong ReadLEB128(Stream s)
        {
            ulong result = 0;
            byte shift = 0;
            while (true)
            {
                int i = s.ReadByte();
                if (i == -1) return result;
                byte b = (byte)i;
                result |= (ulong)((b & 0x7f) << shift);
                if ((b >> 7) == 0)
                    return result;
                shift += 7;
            }
        }

        public static void WriteLEB128(Stream s, int value)
        {
            int temp = value;
            while (temp != 0)
            {
                int val = (temp & 0x7f);
                temp >>= 7;

                if (temp > 0)
                    val |= 0x80;

                s.WriteByte((byte)val);
            }
        }

        public static bool MatchByteArray(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] StringAsByteArray(string str)
        {
            MemoryStream m = new MemoryStream();
            foreach (char c in str)
                m.WriteByte((byte)c);
            return m.ToArray();
        }

        public static string ByteArrayToHexString(byte[] data, int start = 0, int len = 0)
        {
            if (data == null)
                data = new byte[0];
            StringBuilder sb = new StringBuilder();
            if (start == 0)
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
            else
                if (start > 0 && start + len <= data.Length)
                    for (int i = start; i < start + len; i++)
                        sb.Append(data[i].ToString("X2"));
                else
                    return "";
            return sb.ToString();
        }

        public static string ByteArrayAsString(byte[] data)
        {
            if (data == null)
                data = new byte[0];
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append((char)b);
            return sb.ToString();
        }

        public static string ReadUnrealString(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            int len = ReadInt(s);
            for (int i = 0; i < len - 1; i++)
                sb.Append((char)s.ReadByte());
            s.ReadByte();
            return sb.ToString();
        }

        public static byte[] AESDecrypt(byte[] buff, byte[] key)
        {
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.KeySize = 0x100;
            rDel.Key = key;       
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.None; 
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            return cTransform.TransformFinalBlock(buff, 0, buff.Length);
        }
    }
}
