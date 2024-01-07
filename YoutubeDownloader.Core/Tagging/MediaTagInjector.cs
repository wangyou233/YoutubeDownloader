using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Tagging
{
    /// <summary>
    /// 该类用于向媒体文件中注入元数据，包括通用信息、音乐信息以及缩略图。
    /// </summary>
    public class MediaTagInjector
    {
        private readonly MusicBrainzClient _musicBrainz = new MusicBrainzClient();

        /// <summary>
        /// 注入通用元数据（如视频描述和下载信息）到媒体文件中。
        /// </summary>
        /// <param name="mediaFile">目标媒体文件</param>
        /// <param name="video">YouTube视频信息对象</param>
        private void InjectMiscMetadata(MediaFile mediaFile, IVideo video)
        {
            string description = (video as Video)?.Description ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(description))
                mediaFile.SetDescription(description);

            mediaFile.SetComment($@"
Downloaded using YoutubeDownloader (https://github.com/Tyrrrz/YoutubeDownloader)
Video: {video.Title}
Video URL: {video.Url}
Channel: {video.Author.ChannelTitle}
Channel URL: {video.Author.ChannelUrl}");
        }

        /// <summary>
        /// 异步从MusicBrainz API搜索并注入音乐元数据（艺术家、标题等）到媒体文件中。
        /// </summary>
        /// <param name="mediaFile">目标媒体文件</param>
        /// <param name="video">YouTube视频信息对象</param>
        /// <param name="cancellationToken">用于取消操作的令牌</param>
        private async Task InjectMusicMetadataAsync(
            MediaFile mediaFile,
            IVideo video,
            CancellationToken cancellationToken = default)
        {
            var recordings = await _musicBrainz.SearchRecordingsAsync(video.Title, cancellationToken);

            Recording recording = recordings.FirstOrDefault(r =>
                video.Title.IndexOf(r.Title, StringComparison.OrdinalIgnoreCase) >= 0
                && (
                    video.Title.IndexOf(r.Artist, StringComparison.OrdinalIgnoreCase) >= 0
                    || video.Author.ChannelTitle.IndexOf(r.Artist, StringComparison.OrdinalIgnoreCase) >= 0
                )
            );

            if (recording != null)
            {
                mediaFile.SetArtist(recording.Artist);
                mediaFile.SetTitle(recording.Title);

                if (!string.IsNullOrWhiteSpace(recording.ArtistSort))
                    mediaFile.SetArtistSort(recording.ArtistSort);

                if (!string.IsNullOrWhiteSpace(recording.Album))
                    mediaFile.SetAlbum(recording.Album);
            }
        }

        /// <summary>
        /// 异步获取并注入视频缩略图到媒体文件中。
        /// </summary>
        /// <param name="mediaFile">目标媒体文件</param>
        /// <param name="video">YouTube视频信息对象</param>
        /// <param name="cancellationToken">用于取消操作的令牌</param>
        private async Task InjectThumbnailAsync(
            MediaFile mediaFile,
            IVideo video,
            CancellationToken cancellationToken = default)
        {
            string thumbnailUrl = video
                .Thumbnails
                .Where(t => t.TryGetImageFormat()?.Equals("jpg", StringComparison.OrdinalIgnoreCase) == true)
                .OrderByDescending(t => t.Resolution.Area)
                .Select(t => t.Url)
                .FirstOrDefault() ?? $"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg";

            byte[] imageData = await Http.Client.GetByteArrayAsync(thumbnailUrl, cancellationToken);
            mediaFile.SetThumbnail(imageData);
        }

        /// <summary>
        /// 同步执行所有元数据注入任务。
        /// </summary>
        /// <param name="filePath">媒体文件路径</param>
        /// <param name="video">YouTube视频信息对象</param>
        /// <param name="cancellationToken">用于取消操作的令牌</param>
        public async Task InjectTagsAsync(
            string filePath,
            IVideo video,
            CancellationToken cancellationToken = default)
        {
            using var mediaFile = MediaFile.Create(filePath);

            InjectMiscMetadata(mediaFile, video);
            await InjectMusicMetadataAsync(mediaFile, video, cancellationToken);
            await InjectThumbnailAsync(mediaFile, video, cancellationToken);
        }
    }
}