using System;
namespace BiliDanmaku.services
{
    public delegate void DanmakuHandler(Danmaku d); //接受到弹幕后执行
    public delegate void PopHandle(Int32 pop); //房间人气变动

    public interface DanmakuAPi
    {
        void SetRoomId(string roomid);
        void Connect();
        void Close();
        void SetOnConnected(EventHandler e);
        void SetOnDanmaku(DanmakuHandler d);
        void SetOnPop(PopHandle p);
        void SetOnError(EventHandler e);
        void SetOnClose(EventHandler e);
    }
    public class Danmaku
    {
        public string User { get; set; }
        public string Message { get; set; }
    }
}
