#if IOS
using Foundation;
using UIKit;
using UniformTypeIdentifiers;

namespace whitespc.Services;

public partial class FileSaverService : IFileSaverService
{
    public async Task<FileSaveResult> SaveFileAsync(string defaultFileName, byte[] data, string mimeType = "application/pdf")
    {
        var tcs = new TaskCompletionSource<FileSaveResult>();
        
        try
        {
            // Save to temp file first
            var tempPath = Path.Combine(FileSystem.CacheDirectory, defaultFileName);
            await File.WriteAllBytesAsync(tempPath, data);
            
            var tempUrl = NSUrl.FromFilename(tempPath);
            
            // Create document picker in export mode
            var documentPicker = new UIDocumentPickerViewController(new[] { tempUrl }, UIDocumentPickerMode.ExportToService);
            
            documentPicker.DidPickDocumentAtUrls += (sender, e) =>
            {
                if (e.Urls?.Length > 0)
                {
                    var destUrl = e.Urls[0];
                    tcs.TrySetResult(new FileSaveResult(true, destUrl.Path));
                }
                else
                {
                    tcs.TrySetResult(new FileSaveResult(false, null, "No destination selected"));
                }
            };
            
            documentPicker.WasCancelled += (sender, e) =>
            {
                tcs.TrySetResult(new FileSaveResult(false, null, "Save cancelled"));
            };
            
            // Present the picker
            var window = UIApplication.SharedApplication.KeyWindow 
                ?? UIApplication.SharedApplication.Windows.FirstOrDefault();
            var viewController = window?.RootViewController;
            
            if (viewController != null)
            {
                while (viewController.PresentedViewController != null)
                {
                    viewController = viewController.PresentedViewController;
                }
                
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await viewController.PresentViewControllerAsync(documentPicker, true);
                });
            }
            else
            {
                tcs.TrySetResult(new FileSaveResult(false, null, "Could not find view controller"));
            }
        }
        catch (Exception ex)
        {
            tcs.TrySetResult(new FileSaveResult(false, null, ex.Message));
        }
        
        return await tcs.Task;
    }
}
#endif
