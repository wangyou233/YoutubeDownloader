using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading
{
    /// <summary>
    /// 表示用户下载视频时的偏好设置，包括首选容器类型和视频质量。
    /// </summary>
    public record VideoDownloadPreference
    {
        /// <summary>
        /// 用户偏好的媒体容器格式（如MP4或WebM）。
        /// </summary>
        public Container PreferredContainer { get; init; }

        /// <summary>
        /// 用户偏好的视频质量设定。
        /// </summary>
        public VideoQualityPreference PreferredVideoQuality { get; init; }

        /// <summary>
        /// 根据用户的偏好设置，在给定的下载选项中尝试找到最佳匹配项。
        /// </summary>
        /// <param name="options">可供选择的视频下载选项列表。</param>
        /// <returns>与用户偏好最匹配的<see cref="VideoDownloadOption"/>对象；若无满足条件的选项，则返回优先级最高的容器类型的选项，或者在没有符合任何条件的情况下返回null。</returns>
        public VideoDownloadOption? TryGetBestOption(IReadOnlyList<VideoDownloadOption> options)
        {
            // 如果首选容器是音频专用格式，则直接查找对应容器的首个音频选项
            if (PreferredContainer.IsAudioOnly)
                return options.FirstOrDefault(o => o.Container == PreferredContainer);

            // 按照视频质量对所有选项进行排序
            var orderedOptions = options.OrderBy(o => o.VideoQuality).ToArray();

            // 根据用户所选视频质量偏好来筛选最佳选项
            VideoDownloadOption? preferredOption = PreferredVideoQuality switch
            {
                // 最高质量：查找最高分辨率且容器类型相符的选项
                VideoQualityPreference.Highest
                    => orderedOptions.LastOrDefault(o => o.Container == PreferredContainer),

                // 1080p及以下：查找分辨率为1080p或更低、容器类型相符的选项
                VideoQualityPreference.UpTo1080p
                    => orderedOptions
                        .Where(o => o.VideoQuality?.MaxHeight <= 1080)
                        .LastOrDefault(o => o.Container == PreferredContainer),

                // 720p及以下：查找分辨率为720p或更低、容器类型相符的选项 
                VideoQualityPreference.UpTo720p
                    => orderedOptions
                        .Where(o => o.VideoQuality?.MaxHeight <= 720)
                        .LastOrDefault(o => o.Container == PreferredContainer),

                // 480p及以下：查找分辨率为480p或更低、容器类型相符的选项 
                VideoQualityPreference.UpTo480p
                    => orderedOptions
                        .Where(o => o.VideoQuality?.MaxHeight <= 480)
                        .LastOrDefault(o => o.Container == PreferredContainer),

                // 最低质量：查找最低分辨率且容器类型相符的选项
                VideoQualityPreference.Lowest
                    => orderedOptions.LastOrDefault(o => o.Container == PreferredContainer),

                // 非预期值时抛出异常
                _ => throw new InvalidOperationException(
                    $"未知的视频质量偏好设定: '{PreferredVideoQuality}'."
                )
            };

            // 若未找到完全匹配的选项，则退而求其次，选择第一个容器类型相符的选项
            return preferredOption
                ?? orderedOptions.FirstOrDefault(o => o.Container == PreferredContainer);
        }
    }
}