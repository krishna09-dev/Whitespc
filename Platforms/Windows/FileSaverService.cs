#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace whitespc.Services;

public partial class FileSaverService : IFileSaverService
{
    public async Task<FileSaveResult> SaveFileAsync(string defaultFileName, byte[] data, string mimeType = "application/pdf")
    {
        try
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = Path.GetFileNameWithoutExtension(defaultFileName)
            };
            
            // Set file type based on mime type
            var extension = Path.GetExtension(defaultFileName);
            savePicker.FileTypeChoices.Add("PDF Document", new List<string> { extension });
            
            // Get the window handle for the picker
            var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
            {
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(savePicker, hwnd);
            }
            
            var file = await savePicker.PickSaveFileAsync();
            
            if (file != null)
            {
                // Write the data to the file
                await Windows.Storage.FileIO.WriteBytesAsync(file, data);
                return new FileSaveResult(true, file.Path);
            }
            
            return new FileSaveResult(false, null, "Save cancelled");
        }
        catch (Exception ex)
        {
            return new FileSaveResult(false, null, ex.Message);
        }
    }
}
#endif
