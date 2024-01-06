using System.Net;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader
{
    /// <summary>
    /// 应用程序启动引导类，继承自Stylet的Bootstrapper<TShell>，这里TShell是RootViewModel。
    /// 这个类负责配置应用程序的依赖注入容器（IoC Container）、设置全局默认行为以及处理未捕获异常等初始化工作。
    /// </summary>
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            // 调用基类的OnStart方法进行初始化
            base.OnStart();

            // 设置默认的主题为浅色主题，之后会根据加载的设置来更改为首选主题
            App.SetLightTheme();

            // 增加同时最大并发连接数
            ServicePointManager.DefaultConnectionLimit = 20;
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // 首先调用基类的方法完成基本的IoC容器配置
            base.ConfigureIoC(builder);

            // 在IoC容器中注册单例实例：SettingsService
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();

            // 使用抽象工厂模式注册IViewModelFactory接口的实现
            builder.Bind<IViewModelFactory>().ToAbstractFactory();
        }

#if !DEBUG
        // 如果不是调试模式下运行，覆盖OnUnhandledException方法以处理未经处理的Dispatcher异常
        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs args)
        {
            // 先调用基类的方法处理异常
            base.OnUnhandledException(args);

            // 显示一个消息框，包含异常信息
            MessageBox.Show(
                args.Exception.ToString(),
                "Error occurred",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
#endif
    }
}