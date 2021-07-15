using System.Windows;

namespace BiliDanmaku.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ViewModels.MainWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private bool autoscroll = true;

        private void outputbox_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (e.VerticalChange == 0 && autoscroll)
            {
                outputbox.ScrollToEnd();
            }

            if (e.VerticalOffset == outputbox.ScrollableHeight) //滑到底就启用自动滚动
            {
                autoscroll = true;
                outputbox.ScrollToEnd();
            }
            else
            {
                autoscroll = false;
            }
        }
    }
}
