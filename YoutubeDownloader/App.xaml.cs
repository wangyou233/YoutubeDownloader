using System;
using System.Reflection;
using System.Windows.Media;
using MaterialDesignThemes.Wpf; // 引入Material Design in XAML Toolkit库
using YoutubeDownloader.Utils;

namespace YoutubeDownloader
{
    public partial class App
   {
        // 获取当前程序集的信息
        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        // 获取应用程序名称
        public static string Name { get; } = Assembly.GetName().Name!;

        // 获取应用程序版本
        public static Version Version { get; } = Assembly.GetName().Version!;

        // 获取版本字符串（保留3位版本号）
        public static string VersionString { get; } = Version.ToString(3);

        // 项目GitHub地址
        public static string ProjectUrl { get; } = "https://github.com/Tyrrrz/YoutubeDownloader";

        // 获取项目的最新发布版本链接
        public static string LatestReleaseUrl { get; } = ProjectUrl + "/releases/latest";

        // 定义浅色主题，包含基础主题、主色调和强调色
        private static Theme LightTheme { get; } =
            Theme.Create(
                new MaterialDesignLightTheme(), // 基于Material Design的浅色主题
                MediaColor.FromHex("#343838"), // 主色调
                MediaColor.FromHex("#F9A825")   // 强调色
            );

        // 定义深色主题，包含基础主题、主色调和强调色
        private static Theme DarkTheme { get; } =
            Theme.Create(
                new MaterialDesignDarkTheme(), // 基于Material Design的深色主题
                MediaColor.FromHex("#E8E8E8"), // 主色调
                MediaColor.FromHex("#F9A825")  // 强调色
            );

        // 设置应用程序为浅色主题
        public static void SetLightTheme()
        {
            var paletteHelper = new PaletteHelper(); // 创建PaletteHelper对象以修改应用主题
            paletteHelper.SetTheme(LightTheme); // 应用浅色主题

            // 设置不同状态的颜色资源
            Current.Resources["SuccessBrush"] = new SolidColorBrush(Colors.DarkGreen);
            Current.Resources["CanceledBrush"] = new SolidColorBrush(Colors.DarkOrange);
            Current.Resources["FailedBrush"] = new SolidColorBrush(Colors.DarkRed);
        }

        // 设置应用程序为深色主题
        public static void SetDarkTheme()
        {
            var paletteHelper = new PaletteHelper();
            paletteHelper.SetTheme(DarkTheme); // 应用深色主题

            // 设置不同状态的颜色资源
            Current.Resources["SuccessBrush"] = new SolidColorBrush(Colors.LightGreen);
            Current.Resources["CanceledBrush"] = new SolidColorBrush(Colors.Orange);
            Current.Resources["FailedBrush"] = new SolidColorBrush(Colors.OrangeRed);
        }
    }
}