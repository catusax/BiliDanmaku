using Prism.Mvvm;
using Prism.Commands;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using HandyControl.Controls;

namespace ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly BiliDanmaku.services.DanmakuAPi api;
        public DelegateCommand Connect { get; private set; }
        private bool _canConnect;
        public DelegateCommand Clear { get; private set; }
        public DelegateCommand DisConnect { get; private set; }
        private bool _canDisConnect;

        public DelegateCommand<TextBox> RoomIdChanged { get; private set; }

        public MainWindowViewModel(BiliDanmaku.services.DanmakuAPi danmaku)
        {
            Connect = new DelegateCommand(async () => await ConnectDanmaku());
            DisConnect = new DelegateCommand(async () => await DisConnectDanmaku()).ObservesCanExecute(() => CanDisConnect);
            Clear = new DelegateCommand(ClearDanmaku);
            RoomIdChanged = new DelegateCommand<TextBox>(RoomidChanged);

            api = danmaku;
            Pop = "";
            //接收到弹幕的事件
            api.SetOnDanmaku((BiliDanmaku.services.Danmaku danmaku) =>
            {

                _ = ThreadPool.QueueUserWorkItem(delegate
                  {
                      SynchronizationContext.SetSynchronizationContext(new
                          DispatcherSynchronizationContext(Application.Current.Dispatcher));
                      SynchronizationContext.Current.Post(pl =>
                      {
                          //里面写真正的业务内容
                          Danmakus.Add(danmaku);
                          if (Danmakus.Count > 500)
                          {
                              Danmakus.RemoveAt(0); //限制弹幕数量
                          }
                      }, null);
                  });
            });

            //接收到热度信息的事件,已经进入房间
            api.SetOnPop((num) =>
            {
                Pop = $"当前直播间人气值: {num}";
            });

            //连接出错的事件
            api.SetOnError((sender, e) =>
            {
                CanConnect = true;
                CanDisConnect = false;
                CanNotEdit = false;
                Pop = "";
            });

            //断开连接的事件
            api.SetOnClose((sender, e) =>
            {
                CanConnect = true;
                CanDisConnect = false;
                CanNotEdit = false;
                Pop = "";
            });

            api.SetOnConnected((sender, e) =>
            {
                Pop = "连接成功！";
            });

        }

        public ObservableCollection<BiliDanmaku.services.Danmaku> Danmakus { get; set; } = new ObservableCollection<BiliDanmaku.services.Danmaku> { };

        private bool _canNotEdit;

        private string _Pop;
        public string Pop
        {
            get => _Pop;
            set => SetProperty(ref _Pop, value);
        }

        private string roomId;
        public bool CanConnect { get => _canConnect; set => SetProperty(ref _canConnect, value); }
        public bool CanDisConnect { get => _canDisConnect; set => SetProperty(ref _canDisConnect, value); }
        public bool CanNotEdit { get => _canNotEdit; set => _ = SetProperty(ref _canNotEdit, value); }

        public string RoomId
        {
            get => roomId; set => SetProperty(ref roomId, value);
        }

        private async Task ConnectDanmaku()
        {
            CanConnect = false;
            CanDisConnect = true;
            CanNotEdit = true;
            api.SetRoomId(RoomId);
            await Task.Run(() => api.Connect());
        }

        private void ClearDanmaku()
        {
            Danmakus.Clear();
        }

        private async Task DisConnectDanmaku()
        {
            CanDisConnect = false;
            await Task.Run(() =>
            {
                api.Close();
            });
        }

        private void RoomidChanged(TextBox textBox)
        {
            if (textBox.Text == string.Empty) //不能为空
            {
                textBox.IsError = true;
                CanConnect = false;
            }
            else
            {
                textBox.IsError = !textBox.VerifyData();
                CanConnect = !textBox.IsError;
            }
        }
    }
}