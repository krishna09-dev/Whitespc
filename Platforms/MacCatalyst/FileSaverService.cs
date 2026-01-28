using Foundation;
using UIKit;
using Microsoft.Maui.ApplicationModel;
using whitespc.Services;

namespace whitespc.Platforms.MacCatalyst;

public class FileSaverService : IFileSaverService
{
    public async Task<FileSaveResult> SaveFileAsync(
        string defaultFileName,
        byte[] data,
        string mimeType = "application/pdf")
    {
        try
        {
            // Save the bytes FIRST (this can be background)
            var tempPath = Path.Combine(FileSystem.CacheDirectory, defaultFileName);
            await File.WriteAllBytesAsync(tempPath, data);
            var tempUrl = NSUrl.FromFilename(tempPath);

            // Everything UIKit must be on main thread
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var tcs = new TaskCompletionSource<FileSaveResult>();

                var picker = new UIDocumentPickerViewController(
                    new[] { tempUrl },
                    UIDocumentPickerMode.ExportToService
                );

                picker.DidPickDocumentAtUrls += (_, e) =>
                {
                    var picked = e.Urls?.FirstOrDefault();
                    tcs.TrySetResult(new FileSaveResult(true, picked?.Path, null));
                };

                picker.WasCancelled += (_, __) =>
                {
                    tcs.TrySetResult(new FileSaveResult(false, null, "Export cancelled"));
                };

                var vc = GetTopViewController();
                if (vc == null)
                    return new FileSaveResult(false, null, "No active window");

                vc.PresentViewController(picker, true, null);

                return await tcs.Task;
            });
        }
        catch (Exception ex)
        {
            return new FileSaveResult(false, null, ex.ToString());
        }
    }

    private static UIViewController? GetTopViewController()
    {
        var window = UIApplication.SharedApplication
            .ConnectedScenes
            .OfType<UIWindowScene>()
            .SelectMany(s => s.Windows)
            .FirstOrDefault(w => w.IsKeyWindow);

        var root = window?.RootViewController;
        if (root == null) return null;

        while (root.PresentedViewController != null)
            root = root.PresentedViewController;

        return root;
    }
}