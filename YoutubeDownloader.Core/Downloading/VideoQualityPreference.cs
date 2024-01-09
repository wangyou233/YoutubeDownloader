using System;

namespace YoutubeDownloader.Core.Downloading
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Represents the video quality preferences for downloading YouTube videos.
    /// </summary>
    public enum VideoQualityPreference
    {
        /// <summary>
        /// Specifies to download the lowest available video quality.
        /// </summary>
        Lowest,

        /// <summary>
        /// Specifies to download a video quality up to 480p (standard definition).
        /// </summary>
        UpTo480p,

        /// <summary>
        /// Specifies to download a video quality up to 720p (high definition).
        /// </summary>
        UpTo720p,

        /// <summary>
        /// Specifies to download a video quality up to 1080p (full high definition).
        /// </summary>
        UpTo1080p,

        /// <summary>
        /// Specifies to download the highest available video quality.
        /// </summary>
        Highest
    }

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Provides extension methods for the <see cref="VideoQualityPreference"/> enum.
    /// </summary>
    public static class VideoQualityPreferenceExtensions
    {
        /// <summary>
        /// Gets the display name for the specified video quality preference.
        /// </summary>
        /// <param name="preference">The video quality preference.</param>
        /// <returns>The display name for the given preference.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown <paramref name="preference"/> is provided.</exception>
        public static string GetDisplayName(this VideoQualityPreference preference) =>
            preference switch
            {
                VideoQualityPreference.Lowest => "Lowest",
                VideoQualityPreference.UpTo480p => "≤ 480p",
                VideoQualityPreference.UpTo720p => "≤ 720p",
                VideoQualityPreference.UpTo1080p => "≤ 1080p",
                VideoQualityPreference.Highest => "Highest",
                _ => throw new ArgumentOutOfRangeException(nameof(preference))
            };
    }
}