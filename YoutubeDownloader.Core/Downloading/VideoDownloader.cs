using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gress; 
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading
{
    /// <summary>
    /// 该类提供从YouTube下载视频、获取可选下载质量以及包含字幕（如果需要）的功能。
    /// </summary>
    public class VideoDownloader : IDisposable
    {
        private readonly YoutubeClient _youtube; // 使用YoutubeExplode库创建的YouTube客户端实例

        /// <summary>
        /// 初始化一个<see cref="VideoDownloader"/>实例，可以接受初始Cookie列表以便登录和访问受保护内容。
        /// </summary>
        /// <param name="initialCookies">用于身份验证的可选Cookie列表，默认为null表示不使用任何初始Cookie。</param>
        public VideoDownloader(IReadOnlyList<Cookie>? initialCookies = null)
        {
            _youtube = new YoutubeClient(Http.Client, initialCookies ?? Array.Empty<Cookie>());
        }

        /// <summary>
        /// 获取指定视频的所有可用下载选项。
        /// </summary>
        /// <param name="videoId">YouTube视频ID。</param>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        /// <returns>一个包含了所有可能下载选项的只读集合。</returns>
        public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
            VideoId videoId,
            CancellationToken cancellationToken = default
        )
        {
            var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
            return VideoDownloadOption.ResolveAll(manifest);
        }

        /// <summary>
        /// 根据偏好设置获取最佳的下载选项。
        /// </summary>
        /// <param name="videoId">YouTube视频ID。</param>
        /// <param name="preference">用户定义的视频下载优先级。</param>
        /// <param name="cancellationToken">取消操作的令牌。</param>
        /// <returns>与给定偏好最匹配的下载选项。如果没有找到合适的选项，则抛出异常。</returns>
        public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
            VideoId videoId,
            VideoDownloadPreference preference,
            CancellationToken cancellationToken = default
        )
        {
            var options = await GetDownloadOptionsAsync(videoId, cancellationToken);

            return preference.TryGetBestOption(options)
                ?? throw new InvalidOperationException("No suitable download option found.");
        }

        /// <summary>
        /// 下载指定视频到指定文件路径，并根据选项包括字幕。
        /// </summary>
        /// <param name="filePath">输出视频文件的完整路径。</param>
        /// <param name="video">来自YouTube的IVideo对象，包含视频的基本信息。</param>
        /// <param name="downloadOption">选择的下载选项，决定视频质量和格式。</param>
        /// <param name="includeSubtitles">是否将字幕嵌入到视频中，默认为true。</param>
        /// <param name="progress">可选的进度报告接口，用于实时更新下载进度。</param>
        /// <param name="cancellationToken">用于取消下载操作的令牌。</param>
        public async Task DownloadVideoAsync(
            string filePath,
            IVideo video,
            VideoDownloadOption downloadOption,
            bool includeSubtitles = true,
            IProgress<Percentage>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            List<ClosedCaptionTrackInfo> trackInfos = new();

            // 如果需要并支持字幕，则获取视频的字幕信息
            if (includeSubtitles && !downloadOption.Container.IsAudioOnly)
            {
                var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(
                    video.Id,
                    cancellationToken
                );

                trackInfos.AddRange(manifest.Tracks);
            }

            // 确保目标目录存在
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            // 开始下载视频流和字幕，并转换为指定容器格式和编码预设
            await _youtube.Videos.DownloadAsync(
                downloadOption.StreamInfos,
                trackInfos,
                new ConversionRequestBuilder(filePath)
                    .SetContainer(downloadOption.Container)
                    .SetPreset(ConversionPreset.Medium) // 设置转换预设为中等质量
                    .Build(),
                progress?.ToDoubleBased(), // 将进度报告适配器转换为基于double的进度报告（假设Gress库提供了这个方法）
                cancellationToken
            );
        }

        // 以下省略了Dispose()实现，因为示例中未给出
    }
}