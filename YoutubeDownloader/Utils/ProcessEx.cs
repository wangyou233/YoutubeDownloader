using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YoutubeDownloader.Utils
{
    /// <summary>
    /// 提供扩展方法以简化进程启动操作的静态类。
    /// </summary>
    internal static class ProcessEx
    {
        /// <summary>
        /// 启动指定路径的进程，可选地添加命令行参数列表。 
        /// 使用默认进程创建方式（不使用ShellExecute）。
        /// </summary>
        /// <param name="path">要执行的程序或文件的完整路径。</param>
        /// <param name="arguments">传递给进程的命令行参数列表，可选。</param>
        public static void Start(string path, IReadOnlyList<string>? arguments = null)
        {
            // 创建一个新的进程实例，并设置启动信息
            using var process = new Process { StartInfo = new ProcessStartInfo(path) };

            // 如果存在命令行参数，则逐个添加至ArgumentList
            if (arguments is not null)
            {
                foreach (var argument in arguments)
                    process.StartInfo.ArgumentList.Add(argument);
            }

            // 启动进程
            process.Start();
        }

        /// <summary>
        /// 通过ShellExecute启动指定路径的进程，可选地添加命令行参数列表。
        /// 此方法适用于需要操作系统shell提供额外功能的情况，如打开文件、浏览URL等。
        /// </summary>
        /// <param name="path">要执行的程序或文件的完整路径，或特殊协议（如"mailto:"、"http:"等）。</param>
        /// <param name="arguments">传递给进程的命令行参数列表，可选。</param>
        public static void StartShellExecute(string path, IReadOnlyList<string>? arguments = null)
        {
            // 创建一个新的进程实例，启用ShellExecute，并设置启动信息
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(path) { UseShellExecute = true }
            };

            // 如果存在命令行参数，则逐个添加至ArgumentList
            if (arguments is not null)
            {
                foreach (var argument in arguments)
                    process.StartInfo.ArgumentList.Add(argument);
            }

            // 启动进程
            process.Start();
        }
    }
}
