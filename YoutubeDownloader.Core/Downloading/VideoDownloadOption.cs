using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading
{
    public partial record VideoDownloadOption
    {
        public Container Container { get; init; }
        public bool IsAudioOnly { get; init; }
        public IReadOnlyList<IStreamInfo> StreamInfos { get; init; }

        // 使用Lazy属性获取视频质量，懒加载计算
        [Lazy]
        public VideoQuality? VideoQuality =>
            StreamInfos.OfType<IVideoStreamInfo>().MaxBy(s => s.VideoQuality)?.VideoQuality;

        internal static IReadOnlyList<VideoDownloadOption> ResolveAll(StreamManifest manifest)
        {
            var videoAndAudioOptions = new List<VideoDownloadOption>();
            var audioOnlyOptions = new List<VideoDownloadOption>();

            IEnumerable<VideoDownloadOption> GenerateVideoAndAudioOptions()
            {
                foreach (var videoStreamInfo in manifest.GetVideoStreams().OrderByDescending(v => v.VideoQuality))
                {
                    if (videoStreamInfo is MuxedStreamInfo muxedStream)
                    {
                        videoAndAudioOptions.Add(new VideoDownloadOption(
                            muxedStream.Container,
                            false,
                            new[] { muxedStream }
                        ));
                    }
                    else
                    {
                        // 获取匹配的音频流信息
                        var matchingAudioStream = manifest.GetAudioStreams()
                            .OrderByDescending(s => s.Container == videoStreamInfo.Container)
                            .ThenByDescending(s => s is AudioOnlyStreamInfo)
                            .ThenByDescending(s => s.Bitrate)
                            .FirstOrDefault();

                        if (matchingAudioStream != null)
                        {
                            videoAndAudioOptions.Add(new VideoDownloadOption(
                                videoStreamInfo.Container,
                                false,
                                new IStreamInfo[] { videoStreamInfo, matchingAudioStream }
                            ));
                        }
                    }
                }
                return videoAndAudioOptions;
            }

            IEnumerable<VideoDownloadOption> GenerateAudioOnlyOptions()
            {
                // WebM-based audio-only containers
                {
                    var webMAudio = manifest.GetAudioStreams()
                        .OrderByDescending(s => s.Container == Container.WebM)
                        .ThenByDescending(s => s is AudioOnlyStreamInfo)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                    if (webMAudio != null)
                    {
                        audioOnlyOptions.Add(new VideoDownloadOption(
                            Container.WebM,
                            true,
                            new[] { webMAudio }
                        ));

                        audioOnlyOptions.Add(new VideoDownloadOption(
                            Container.Mp3,
                            true,
                            new[] { webMAudio }
                        ));

                        audioOnlyOptions.Add(new VideoDownloadOption(
                            new Container("ogg"),
                            true,
                            new[] { webMAudio }
                        ));
                    }
                }

                // Mp4-based audio-only containers
                {
                    var mp4Audio = manifest.GetAudioStreams()
                        .OrderByDescending(s => s.Container == Container.Mp4)
                        .ThenByDescending(s => s is AudioOnlyStreamInfo)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                    if (mp4Audio != null)
                    {
                        audioOnlyOptions.Add(new VideoDownloadOption(
                            Container.Mp4,
                            true,
                            new[] { mp4Audio }
                        ));
                    }
                }

                return audioOnlyOptions;
            }

            // 去重并返回下载选项列表
            var optionsComparer = EqualityComparer<VideoDownloadOption>.Create(
                (x, y) => x.VideoQuality == y.VideoQuality && x.Container == y.Container,
                x => HashCode.Combine(x.VideoQuality?.GetHashCode() ?? 0, x.Container.GetHashCode())
            );

            var allOptions = new HashSet<VideoDownloadOption>(optionsComparer);

            foreach (var option in GenerateVideoAndAudioOptions().Concat(GenerateAudioOnlyOptions()))
            {
                allOptions.Add(option);
            }

            return allOptions.ToArray();
        }
    }
}