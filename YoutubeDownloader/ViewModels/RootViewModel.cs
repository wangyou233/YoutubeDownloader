using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels
{
    public class RootViewModel : Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;

        public SnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public DashboardViewModel Dashboard { get; }

        public RootViewModel(
            IViewModelFactory viewModelFactory,
            DialogManager dialogManager,
            SettingsService settingsService,
            UpdateService updateService
        )
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _updateService = updateService;

            Dashboard = viewModelFactory.CreateDashboardViewModel();

            DisplayName = $"{App.Name} v{App.VersionString}";
        }

        private async Task HandleUkraineSupportAsync()
        {
            if (!_settingsService.IsUkraineSupportMessageEnabled)
                return;

            var messageBox = _viewModelFactory.CreateMessageBoxViewModel(
                "Thank you for supporting Ukraine!",
                $@"As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

                Click LEARN MORE to find ways that you can help.",
                "LEARN MORE",
                "CLOSE"
            );

            // Disable the message and save the setting
            _settingsService.IsUkraineSupportMessageEnabled = false;
            await _settingsService.SaveAsync();

            if (await _dialogManager.ShowDialogAsync(messageBox) == true)
            {
                ProcessEx.StartShellExecute("https://tyrrrz.me/ukraine?source=youtubedownloader");
            }
        }

        private async Task CheckForAndHandleUpdatesAsync()
        {
            try
            {
                var updateVersion = await _updateService.CheckForUpdatesAsync();
                if (updateVersion is null)
                    return;

                Notifications.Enqueue($"Downloading update to {App.Name} v{updateVersion}...");
                await _updateService.PrepareUpdateAsync(updateVersion);

                Notifications.Enqueue(
                    $"Update has been downloaded and will be installed when you exit",
                    "INSTALL NOW",
                    () =>
                    {
                        _updateService.FinalizeUpdate(true);
                        RequestClose();
                    },
                    TimeSpan.FromMinutes(5)
                );
            }
            catch
            {
                Notifications.Enqueue("Failed to perform application update", TimeSpan.FromSeconds(5));
            }
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();

            await _settingsService.LoadAsync();

            // Synchronize theme with settings
            App.SetTheme(_settingsService.IsDarkModeEnabled ? Theme.Dark : Theme.Light);

            // Show changelog on successful update
            if (_settingsService.LastAppVersion != App.Version)
            {
                Notifications.Enqueue(
                    $"Successfully updated to {App.Name} v{App.VersionString}",
                    "WHAT'S NEW",
                    () => ProcessEx.StartShellExecute(App.LatestReleaseUrl),
                    TimeSpan.FromSeconds(5)
                );

                _settingsService.LastAppVersion = App.Version;
                await _settingsService.SaveAsync();
            }

            await HandleUkraineSupportAsync();
            await CheckForAndHandleUpdatesAsync();
        }

        protected override async void OnClose()
        {
            Dashboard.CancelAllDownloads();

            await _settingsService.SaveAsync();
            _updateService.FinalizeUpdate(false);

            base.OnClose();
        }
    }
}