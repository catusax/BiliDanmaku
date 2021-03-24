using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace BiliDanmaku.services.DanmakuApi
{
    class BiliDanmakuApi: DanmakuAPi
    {
        private string Roomid;
        private DanmakuHandler onDanmaku;
        private EventHandler ConnectHandle;
        private PopHandle popHandle;
        private EventHandler OnError;
        private EventHandler OnClose;

        private readonly WebSocket ws;

        public BiliDanmakuApi()
        {
            ws = new WebSocket("wss://broadcastlv.chat.bilibili.com/sub");
        }

        public void Connect()
        {
            ws.OnOpen += async (sender, e) =>
            {
                var room = $"{{\"roomid\":{Roomid},\"protover\": 1}}";
                var s = Encoder.Encode(room, 7);
                Console.WriteLine(room);
                ws.Send(s);
                await KeepAlive();
                ConnectHandle?.Invoke(sender, e);
            };
            ws.OnMessage += (sender, e) =>
            {
                var resp = Encoder.Decode(e.RawData);
                if (resp.num != 0)
                    popHandle?.Invoke(resp.num);
                if (resp.body != null)
                {
                    foreach (string body in resp.body)
                    {
                        //Console.WriteLine($"Body::   {body}");
                        Message msg = JsonSerializer.Deserialize<Message>(body);
                        //Console.WriteLine($"type::{msg.cmd}");
                        if (msg.cmd == "DANMU_MSG")
                        {
                            var User = JsonSerializer.Deserialize<List<Object>>(msg.info[2].ToString());
                            var danmaku = new Danmaku
                            {
                                User = User[1].ToString(),
                                Message = msg.info[1].ToString()
                            };
                            onDanmaku?.Invoke(danmaku);
                            //Console.WriteLine($"Message::  {danmaku.user}: {danmaku.message}");
                        }
                    }
                }
                //Console.WriteLine(e.ToString());
            };
            ws.OnError += (sender, e) => { OnError?.Invoke(sender, e); };

            ws.OnClose += (sender, e) => { OnClose?.Invoke(sender, e); };

            ws.Connect();
            //ws.Send("BALUS");
        }

        private async Task KeepAlive()
        {
            while (true) //发送心跳包保持连接
            {
                var ss = Encoder.Encode("", 2);
                try
                {
                    ws.Send(ss);
                }
                catch (Exception)
                {
                    //连接断开,退出线程
                    break;
                }
                await Task.Delay(3000);
            }
        }

        public void SetRoomId(string roomid)
        {
            Roomid = roomid;
        }

        public void SetOnDanmaku(DanmakuHandler d)
        {
            onDanmaku += d;
        }

        void DanmakuAPi.SetOnPop(PopHandle p)
        {
            popHandle += p;
        }

        public void SetOnError(EventHandler e)
        {
            OnError = e;
        }

        public void SetOnClose(EventHandler e)
        {
            OnClose = e;
        }

        public void Close()
        {
            ws.Close();
        }

        public void SetOnConnected(EventHandler e)
        {
            ConnectHandle = e;
        }
    }

    public class Message
    {
        public string cmd { get; set; }
        public List<Object> info { get; set; }
    }
}