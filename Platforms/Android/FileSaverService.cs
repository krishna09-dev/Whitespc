#if ANDROID
using Android.App;
using Android.Content;

namespace whitespc.Services;

public partial class FileSaverService : IFileSaverService
{
    private static TaskCompletionSource<FileSaveResult>? _tcs;
    private static byte[]? _pendingData;
    
    public async Task<FileSaveResult> SaveFileAsync(string defaultFileName, byte[] data, string mimeType = "application/pdf")
    {
        _tcs = new TaskCompletionSource<FileSaveResult>();
        _pendingData = data;
        
        try
        {
            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType(mimeType);
            intent.PutExtra(Intent.ExtraTitle, defaultFileName);
            
            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                return new FileSaveResult(false, null, "Could not get current activity");
            }
            
            activity.StartActivityForResult(intent, 1001);
        }
        catch (Exception ex)
        {
            _tcs.TrySetResult(new FileSaveResult(false, null, ex.Message));
        }
        
        return await _tcs.Task;
    }
    
    public static async Task HandleActivityResult(int requestCode, Android.App.Result resultCode, Intent? data)
    {
        if (requestCode != 1001 || _tcs == null) return;
        
        if (resultCode == Android.App.Result.Ok && data?.Data != null && _pendingData != null)
        {
            try
            {
                var uri = data.Data;
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                
                using var outputStream = context.ContentResolver?.OpenOutputStream(uri);
                if (outputStream != null)
                {
                    await outputStream.WriteAsync(_pendingData);
                    await outputStream.FlushAsync();
                    _tcs.TrySetResult(new FileSaveResult(true, uri.Path));
                }
                else
                {
                    _tcs.TrySetResult(new FileSaveResult(false, null, "Could not open output stream"));
                }
            }
            catch (Exception ex)
            {
                _tcs.TrySetResult(new FileSaveResult(false, null, ex.Message));
            }
        }
        else
        {
            _tcs.TrySetResult(new FileSaveResult(false, null, "Save cancelled"));
        }
        
        _pendingData = null;
    }
}
#endif
