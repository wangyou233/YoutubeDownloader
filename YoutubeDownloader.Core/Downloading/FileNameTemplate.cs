using System;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public class FileNameTemplate
{
    
    /// <summary>
    /// 根据给定的模板字符串、视频信息、容器名称以及可选的数字编号，生成并返回一个格式化后的文件名。
    /// </summary>
    /// <param name="template">基础模板字符串，其中包含用于替换的占位符。</param>
    /// <param name="video">IVideo 实例，提供视频的相关属性如ID、标题、作者等信息。</param>
    /// <param name="container">Container 实例，提供文件扩展名（容器名称）。</param>
    /// <param name="number">可选的数字编号，用于在文件名中进行标识。默认为null。</param>
    /// <returns>格式化后的文件名，已对特殊字符进行转义，并添加了容器名称作为扩展名。</returns>
    public static string Apply(
        string template,
        IVideo video,
        Container container,
        string? number = null
    ) =>
        PathEx.EscapeFileName(
            // 替换模板中的占位符，生成最终文件名
            template
                .Replace("$numc", number ?? "", StringComparison.Ordinal)  // 替换数字编号（完整显示）
                .Replace("$num", number is not null ? $"[{number}]" : "", StringComparison.Ordinal) // 替换带方括号的数字编号
                .Replace("$id", video.Id, StringComparison.Ordinal)       // 替换视频ID
                .Replace("$title", video.Title, StringComparison.Ordinal)   // 替换视频标题
                .Replace("$author", video.Author.ChannelTitle, StringComparison.Ordinal) // 替换作者频道标题
                .Replace(
                    "$uploadDate",
                    (video as Video)?.UploadDate.ToString("yyyy-MM-dd") ?? "",
                    StringComparison.Ordinal // 替换上传日期，格式为"年-月-日"
                )
                .Trim() // 移除开头和结尾的空白字符
                + '.' // 添加分隔符（点号）
                + container.Name // 添加容器名称（扩展名）
        );
}
