using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JsonExtensions.Http; // 假设这是用于发送HTTP请求并处理JSON响应的库
using JsonExtensions.Reading; // 假设这是提供扩展方法以便更方便地读取JSON数据的库
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Core.Tagging
{
    /// <summary>
    /// 音乐标签服务客户端，用于从MusicBrainz API搜索音乐录音信息。
    /// </summary>
    internal class MusicBrainzClient
    {
        // 限制每秒最多4个请求
        private readonly ThrottleLock _throttleLock = new(TimeSpan.FromSeconds(1.0 / 4));

        /// <summary>
        /// 异步搜索与给定查询匹配的音乐录音，并返回一个包含MusicBrainzRecording对象的异步可枚举集合。
        /// </summary>
        /// <param name="query">要搜索的音乐录音名称或相关描述。</param>
        /// <param name="cancellationToken">用于取消搜索操作的令牌。</param>
        public async IAsyncEnumerable<MusicBrainzRecording> SearchRecordingsAsync(
            string query,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            // 构造MusicBrainz API请求URL
            var url =
                "https://musicbrainz.org/ws/2/recording/"
                + "?version=2"
                + "&fmt=json"
                + "&dismax=true"
                + "&limit=100"
                + $"&query={Uri.EscapeDataString(query)}";

            // 等待速率限制锁以控制请求频率
            await _throttleLock.WaitAsync(cancellationToken);

            // 发送GET请求获取JSON响应
            var json = await Http.Client.GetJsonAsync(url, cancellationToken);

            // 获取JSON中"recordings"数组
            var recordingsJson = json.GetPropertyOrNull("recordings")?.EnumerateArrayOrNull() ?? Enumerable.Empty<JToken>();

            foreach (var recordingJson in recordingsJson)
            {
                // 解析艺术家名称
                var artist = recordingJson
                    .GetPropertyOrNull("artist-credit")
                    ?.EnumerateArrayOrNull()
                    ?.FirstOrDefault()
                    .GetPropertyOrNull("name")
                    ?.GetNonWhiteSpaceStringOrNull();

                if (string.IsNullOrWhiteSpace(artist))
                    continue;

                // 解析艺术家排序名
                var artistSort = recordingJson
                    .GetPropertyOrNull("artist-credit")
                    ?.EnumerateArrayOrNull()
                    ?.FirstOrDefault()
                    .GetPropertyOrNull("artist")
                    ?.GetPropertyOrNull("sort-name")
                    ?.GetNonWhiteSpaceStringOrNull();

                // 解析录音标题
                var title = recordingJson.GetPropertyOrNull("title")?.GetNonWhiteSpaceStringOrNull();

                if (string.IsNullOrWhiteSpace(title))
                    continue;

                // 解析录音所在的专辑标题
                var album = recordingJson
                    .GetPropertyOrNull("releases")
                    ?.EnumerateArrayOrNull()
                    ?.FirstOrDefault()
                    .GetPropertyOrNull("title")
                    ?.GetNonWhiteSpaceStringOrNull();

                // 创建并返回MusicBrainzRecording实例
                yield return new MusicBrainzRecording(artist, artistSort, title, album);
            }
        }
    }
}