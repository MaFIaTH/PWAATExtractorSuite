using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
    }
}