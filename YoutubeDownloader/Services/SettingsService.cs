using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cogwheel;
using Microsoft.Win32;
using PropertyChanged;
using YoutubeDownloader.Core.Downloading;
using Container = YoutubeExplode.Videos.Streams.Container;

namespace YoutubeDownloader.Services
{
    [AddINotifyPropertyChangedInterface]
    public partial class SettingsService : SettingsBase, INotifyPropertyChanged
    {
        public SettingsService() : base(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat"))
        {
            // Default constructor
        }

        // Properties for various settings
        public bool IsUkraineSupportMessageEnabled { get; set; } = true;
        public bool IsAutoUpdateEnabled { get; set; } = true;
        public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();
        public bool IsAuthPersisted { get; set; } = true;
        public bool ShouldInjectSubtitles { get; set; } = true;
        public bool ShouldInjectTags { get; set; } = true;
        public bool ShouldSkipExistingFiles { get; set; }
        public string FileNameTemplate { get; set; } = "$title";
        public int ParallelLimit { get; set; } = 2;
        public Version? LastAppVersion { get; set; }
        public IReadOnlyList<Cookie>? LastAuthCookies { get; set; }
        
        // Custom serialization for Container struct using a custom JsonConverter
        [JsonConverter(typeof(ContainerJsonConverter))]
        public Container LastContainer { get; set; } = Container.Mp4;
        
        public VideoQualityPreference LastVideoQualityPreference { get; set; } =
            VideoQualityPreference.Highest;

        public override void Save()
        {
            // Temporarily clear the cookies if they are not supposed to be persisted
            Cookie[]? lastAuthCookiesArray = null;
            if (!IsAuthPersisted && LastAuthCookies != null)
                lastAuthCookiesArray = LastAuthCookies.ToArray();

            LastAuthCookies = null;
            base.Save();

            // Restore the original value of LastAuthCookies
            LastAuthCookies = lastAuthCookiesArray?.AsReadOnly();
        }

        private static bool IsDarkModeEnabledByDefault()
        {
            try
            {
                var registryKey = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
                    false
                );

                if (registryKey is not null && registryKey.GetValue("AppsUseLightTheme") is int themeValue)
                    return themeValue == 0;

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Nested class for JSON serialization/deserialization of Container enum
        private sealed class ContainerJsonConverter : JsonConverter<Container>
        {
            public override Container Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "Name")
                        {
                            reader.Read();
                            if (reader.TokenType == JsonTokenType.String && !string.IsNullOrWhiteSpace(reader.GetString()))
                            {
                                return new Container(reader.GetString());
                            }
                        }
                    }
                }

                throw new InvalidOperationException($"Invalid JSON for type '{typeToConvert.FullName}'.");
            }

            public override void Write(Utf8JsonWriter writer, Container value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Name", value.Name);
                writer.WriteEndObject();
            }
        }
    }
}