using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Stylet;

namespace YoutubeDownloader.ViewModels.Framework
{
    public class DialogManager : IDisposable
    {
        private readonly SemaphoreSlim _dialogSemaphore = new(1, 1);

        public DialogManager(IViewManager viewManager)
        {
            ViewManager = viewManager;
        }

        private IViewManager ViewManager { get; }

        public async ValueTask<T?> ShowDialogAsync<T>(DialogScreen<T> dialogScreen)
        {
            var view = ViewManager.CreateAndBindViewForModelIfNecessary(dialogScreen);

            void OnDialogOpened(object? sender, DialogOpenedEventArgs args)
            {
                void OnScreenClosed(object? closeSender, EventArgs closeArgs)
                {
                    try
                    {
                        args.Session.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore if  dialog is already being closed
                    }

                    dialogScreen.Closed -= OnScreenClosed;
                }

                dialogScreen.Closed += OnScreenClosed;
            }

            await _dialogSemaphore.WaitAsync();
            try
            {
                await DialogHost.Show(view, OnDialogOpened);
                return dialogScreen.DialogResult;
            }
            finally
            {
                _dialogSemaphore.Release();
            }
        }

        public string? PromptSaveFilePath(string filter = "All files|*.*", string defaultFilePath = "")
        {
            using var dialog = new SaveFileDialog
            {
                Filter = filter,
                AddExtension = true,
                FileName = defaultFilePath,
                DefaultExt = Path.GetExtension(defaultFilePath)
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? PromptDirectoryPath(string defaultDirPath = "")
        {
            using var dialog = new OpenFolderDialog { InitialDirectory = defaultDirPath };
            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public void Dispose()
        {
            _dialogSemaphore.Dispose();
        }
    }
}