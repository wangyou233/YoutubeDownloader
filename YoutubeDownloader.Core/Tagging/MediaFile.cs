using System;
using TagLib;
using TagFile = TagLib.File;

namespace YoutubeDownloader.Core.Tagging;

internal partial class MediaFile(TagFile file) : IDisposable
{
    public void SetThumbnail(byte[] thumbnailData) =>
        file.Tag.Pictures = new IPicture[] { new Picture(thumbnailData) };
    ///设置Artist
    public void SetArtist(string artist) => file.Tag.Performers = new[] { artist };
    
    ///设置ArtistSort
    public void SetArtistSort(string artistSort) => file.Tag.PerformersSort = new[] { artistSort };

    ///设置Title
    public void SetTitle(string title) => file.Tag.Title = title;
    ///设置Album
    public void SetAlbum(string album) => file.Tag.Album = album;
    ///设置Description
    public void SetDescription(string description) => file.Tag.Description = description;
    ///设置Comment
    public void SetComment(string comment) => file.Tag.Comment = comment;

    public void Dispose()
    {
        file.Tag.DateTagged = DateTime.Now;
        file.Save();
        file.Dispose();
    }
}

internal partial class MediaFile
{
    public static MediaFile Create(string filePath) => new(TagFile.Create(filePath));
}
