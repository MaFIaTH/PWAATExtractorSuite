using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using PWAATExtractorSuite.Models;

namespace PWAATExtractorSuite.ViewModels.Dialogs;

public static class DialogExtensions
{
    extension(IDialogService service)
    {
        public async Task ShowNotificationDialog(string title, string message)
        {
            var vm = service.GetDialog<NotificationDialogViewModel>();
            vm.Title.Value = title;
            vm.Message.Value = message;
            await DialogHost.Show(vm);
        }

        public async Task<string?> ShowWizardDialog(ExtractorType extractorType)
        {
            var vm = service.GetDialog<WizardDialogViewModel>();
            vm.ExtractorType.Value = extractorType;
            var result = await DialogHost.Show(vm);
            if (result is string path)
            {
                return path;
            }
            return null;
        }
        
        public async Task ShowAboutDialog()
        {
            var vm = service.GetDialog<AboutDialogViewModel>();
            await DialogHost.Show(vm);
        }
        
        public async Task<ConfirmationDialogResult?> ShowConfirmationDialog(string title, string message,
            bool hasCancel, string? yesText = null, string? noText = null, string? cancelText = null)
        {
            var vm = service.GetDialog<ConfirmationDialogViewModel>();
            vm.Title.Value = title;
            vm.Message.Value = message;
            vm.HasCancel.Value = hasCancel;
            if (yesText is not null)
            {
                vm.YesButtonText.Value = yesText;
            }
            if (noText is not null)
            {
                vm.NoButtonText.Value = noText;
            }
            if (cancelText is not null)
            {
                vm.CancelButtonText.Value = cancelText;
            }
            var result = await DialogHost.Show(vm);
            if (result is ConfirmationDialogResult dialogResult)
            {
                return dialogResult;
            }
            return null;
        }
        
        public async ValueTask<string?> PickSingleFile(FilePickerFileType[]? filter = null, string? startingFolderPath = null)
        {
            var startFolder = await service.StorageProvider.TryGetFolderFromPathAsync(startingFolderPath ?? string.Empty);
            var filterType = filter ??
            [
                new FilePickerFileType("All Files")
                {
                    Patterns = ["*"],
                    MimeTypes = ["*/*"]
                }
            ];
            var result = await service.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Select a file",
                AllowMultiple = false,
                FileTypeFilter = filterType,
                SuggestedStartLocation = startFolder
            });
            if (result.Count == 0)
            {
                Console.WriteLine("File picker cancelled");
                return null;
            }
            var path = result[0].Path.LocalPath;
            if (!File.Exists(path))
            {
                await service.ShowNotificationDialog("File Not Found", $"""The selected file "{path}" does not exist.""");
                Console.WriteLine($"File does not exist: {path}");
                return null;
            }
            Console.WriteLine($"File selected: {path}");
            return path;
        }
    }
}