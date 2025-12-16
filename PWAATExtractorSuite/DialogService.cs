using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace PWAATExtractorSuite;

public interface IDialogService
{
    IStorageProvider StorageProvider { get; }
    TViewModel GetDialog<TViewModel>();
    ValueTask<SaveFilePickerResult> SaveFilePickerAsync(FilePickerSaveOptions options);
    ValueTask<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);
    ValueTask<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options);
}

public class DialogService(Func<Type, object> viewModelFactory, Func<IStorageProvider> storageProviderFactory) : IDialogService
{
    public IStorageProvider StorageProvider => storageProviderFactory.Invoke();
    public TViewModel GetDialog<TViewModel>()
    {
        var viewModel = viewModelFactory(typeof(TViewModel));
        if (viewModel is TViewModel typedViewModel)
        {
            return typedViewModel;
        }
        throw new InvalidOperationException($"Could not create dialog of type {typeof(TViewModel).FullName}");
    }
    
    public async ValueTask<SaveFilePickerResult> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        var storageProvider = storageProviderFactory.Invoke();
        return await storageProvider.SaveFilePickerWithResultAsync(options);
    }
    
    public async ValueTask<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        var storageProvider = storageProviderFactory.Invoke();
        var filePickerResult = await storageProvider.OpenFilePickerAsync(options);
        return filePickerResult;
    }

    public async ValueTask<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
    {
        var storageProvider = storageProviderFactory.Invoke();
        var folderPickerResult = await storageProvider.OpenFolderPickerAsync(options);
        return folderPickerResult;
    }
}