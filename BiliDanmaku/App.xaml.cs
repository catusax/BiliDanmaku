using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;
using Prism.Unity;


namespace BiliDanmaku
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.Register<Services.ICustomerStore, Services.DbCustomerStore>();
            // register other needed services here
            containerRegistry.Register<services.DanmakuAPi,services.DanmakuApi.BiliDanmakuApi>();
        }

        protected override Window CreateShell()
        {
            Views.MainWindow w = Container.Resolve<Views.MainWindow>();
            return w;
        }
    }
}
