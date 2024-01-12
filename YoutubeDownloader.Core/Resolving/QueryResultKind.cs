namespace YoutubeDownloader.Core.Resolving;

public enum QueryResultKind
{
    Video,      // Represents a result of type "Video".
    Playlist,   // Represents a result of type "Playlist".
    Channel,    // Represents a result of type "Channel".
    Search,     // Represents a result of type "Search".
    Aggregate   // Represents a result of type "Aggregate".
}
