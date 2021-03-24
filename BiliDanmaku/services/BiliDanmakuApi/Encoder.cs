using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BiliDanmaku.services.DanmakuApi
{
    class Encoder
    {

        static private int ReadInt(byte[] data, int start, int len)
        {
            var buffer = Array.ConvertAll<byte, int>(data, ss => ss);
            var result = 0;
            for (int i = len - 1; i >= 0; i--)
            {
                result += (int)Math.Pow(256, len - i - 1) * buffer[start + i];
            }
            return result;
        }

        static private void WriteInt(ref int[] buffer, int start, int len, int value)
        {
            for (int i = 0; i < len; i++)
            {
                buffer[start + i] = (int)(value / Math.Pow(256, len - i - 1));
            }
        }


        static public byte[] Encode(string s, int op)
        {
            var data = Encoding.UTF8.GetBytes(s);
            var packetLen = 16 + data.Length;
            int[] header = { 0, 0, 0, 0, 0, 16, 0, 1, 0, 0, 0, op, 0, 0, 0, 1 };
            WriteInt(ref header, 0, 4, packetLen);
            var headerbyte = Array.ConvertAll<int, byte>(header, ss => (byte)ss);
            return Byteplus(headerbyte, data);
        }

        static public Result Decode(byte[] data)
        {
            var result = new Result
            {
                packetLen = ReadInt(data, 0, 4),
                headerLen = ReadInt(data, 4, 2),
                op = ReadInt(data, 8, 4),
                Ver = ReadInt(data, 6, 2)
            };
            var Decompressed = Array.Empty<byte>();

            var headerLen = 16;
            //var totalpacketLen = ReadInt(data, 0, 4);
            var fullBody = data[(headerLen)..];

            switch (result.Ver)
            {
                case 0: //json
                    Decompressed = fullBody;
                    break;
                //case 1: //房间人气
                //    result.num = BitConverter.ToInt32(fullBody);
                //    break;
                case 2: //zlib
                    Stream stream = new MemoryStream(fullBody);
                    Decompressed = DecompressZlib(stream);
                    break;
            }

            switch (result.op)
            {
                case 3:
                    if (BitConverter.IsLittleEndian) //在小端的CPU上需要反转字节序
                        Array.Reverse(fullBody);
                    result.num = BitConverter.ToInt32(fullBody);
                    break;
                case 5:
                    result.body = new List<string> { };
                    //Console.WriteLine($"rawdata::{BitConverter.ToString(Decompressed)}");
                    //Console.WriteLine($"rawdata::{Encoding.UTF8.GetString(Decompressed)}\n");
                    var offset = 0;
                    while (offset < Decompressed.Length)
                    {
                        var packetLen = ReadInt(Decompressed, offset, 4);
                        try
                        {
                            var body = Decompressed[(offset + headerLen)..(offset + packetLen)];
                            result.body.Add(Encoding.UTF8.GetString(body));
                        }
                        catch (Exception)
                        {
                            break;
                        }
                        //Console.WriteLine($"body:::{Encoding.UTF8.GetString(body)}");
                        offset += packetLen;
                    }
                    break;
            }

            return result;
        }

        public static byte[] Byteplus(byte[] a, byte[] b)
        {
            byte[] nCon = new byte[a.Length + b.Length];
            a.CopyTo(nCon, 0);
            b.CopyTo(nCon, a.Length);
            return nCon;
        }

        public struct Result
        {
            public int packetLen; //数据包长度
            public int headerLen; //数据包头部长度（固定为 16）
            public int Ver; //协议版本
            public int op; //操作类型
            //public int seq;
            public Int32 num; //
            public List<string> body; //json主体
        }

        public static byte[] DecompressZlib(Stream source)
        {
            byte[] result = null;
            source.Seek(2, SeekOrigin.Begin); //跳过两个字节
            try
            {
                using MemoryStream outStream = new MemoryStream();
                using (DeflateStream inf = new DeflateStream(source, CompressionMode.Decompress))
                {
                    inf.CopyTo(outStream);
                }
                result = outStream.ToArray();
            }
            catch (Exception)
            {
                throw (new Exception("解压失败"));
            }

            return result;
        }
    }
}
