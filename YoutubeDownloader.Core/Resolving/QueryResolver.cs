using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving
{
    /// <summary>
    /// Resolves YouTube queries (video URLs, channel URLs, playlist URLs, usernames, handles, and search keywords) into a structured QueryResult. 
    /// </summary>
    public class QueryResolver : IQueryResolver
    {
        private readonly YoutubeClient _youtube;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResolver"/> class with an optional set of initial cookies for authentication purposes. 
        /// </summary>
        /// <param name="initialCookies">A list of cookies to use when initializing the YouTube client. If null or empty, no cookies will be used.</param>
        public QueryResolver(IReadOnlyList<Cookie>? initialCookies = null)
        {
            var httpClient = Http.Client; // Assuming Http is a static class exposing a configured HttpClient
            _youtube = new YoutubeClient(httpClient, initialCookies ?? Array.Empty<Cookie>());
        }

        /// <summary>
        /// Resolves a single YouTube query into a <see cref="QueryResult"/>.
        /// </summary>
        /// <param name="query">The YouTube query, which could be a video URL, playlist URL, channel URL, username, handle, or search keyword.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation .</param>
        /// <returns>A <see cref="QueryResult"/> representing the resolved content.</returns>
        public async Task<QueryResult> ResolveAsync(
            string query,
            CancellationToken cancellationToken = default
        )
        {
            // Determine if the query is a well-formed URL
            bool isUrl = Uri.IsWellFormedUriString(query, UriKind.Absolute);

            // Handle different types of YouTube resources based on the query
            if (isUrl)
            {
                // Playlist
                if (PlaylistId.TryParse(query) is { } playlistId)
                {
                    var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
                    var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);
                    return new QueryResult(QueryResultKind.Playlist, $"Playlist: {playlist.Title}", videos);
                }

                // Video
                if (VideoId.TryParse(query) is { } videoId)
                {
                    var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
                    return new QueryResult(QueryResultKind.Video, video.Title, new[] { video });
                }

                // Channel (by ID)
                if (ChannelId.TryParse(query) is { } channelId)
                {
                    var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
                    var uploads = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);
                    return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", uploads);
                }

                // Channel (by handle)
                if (ChannelHandle.TryParse(query) is { } channelHandle)
                {
                    var channel = await _youtube.Channels.GetByHandleAsync(channelHandle, cancellationToken);
                    var uploads = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);
                    return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", uploads);
                }

                // Channel (by username)
                if (UserName.TryParse(query) is { } userName)
                {
                    var channel = await _youtube.Channels.GetByUserAsync(userName, cancellationToken);
                    var uploads = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);
                    return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", uploads);
                }

                // Channel (by slug)
                if (ChannelSlug.TryParse(query) is { } channelSlug)
                {
                    var channel = await _youtube.Channels.GetBySlugAsync(channelSlug, cancellationToken);
                    var uploads = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);
                    return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", uploads);
                }
            }

            // Search
            {
                var searchResults = await _youtube
                    .Search.GetVideosAsync(query, cancellationToken)
                    .CollectAsync(20);

                return new QueryResult(QueryResultKind.Search, $"Search: {query}", searchResults);
            }
        }

        /// <summary>
        /// Resolves multiple YouTube queries asynchronously and combines their results into a single <see cref="QueryResult"/>.
        /// </summary>
        /// <param name="queries">A list of YouTube queries to resolve.</param>
        /// <param name="progress">An optional progress reporter to track the completion percentage of resolving all queries.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="QueryResult"/> containing an aggregate of all resolved videos from the provided queries.</returns>
        public async Task<QueryResult> ResolveAsync(
            IReadOnlyList<string> queries,
            IProgress<Percentage>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            if (queries.Count == 1)
                return await ResolveAsync(queries.Single(), cancellationToken);

            var videos = new List<IVideo>();
            var videoIds = new HashSet<VideoId>();

            int completed = 0;
            foreach (var query in queries)
            {
                var result = await ResolveAsync(query, cancellationToken);

                foreach (var video in result.Videos)
                {
                    if (videoIds.Add(video.Id))
                        videos.Add(video);
                }

                progress?.Report(Percentage.FromFraction(1.0 * ++completed / queries.Count));
            }

            return new QueryResult(QueryResultKind.Aggregate, $"{queries.Count} queries", videos);
        }
    }
}