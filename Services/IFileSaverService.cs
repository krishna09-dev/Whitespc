namespace whitespc.Services;

public interface IFileSaverService
{
    Task<FileSaveResult> SaveFileAsync(string defaultFileName, byte[] data, string mimeType = "application/pdf");
}

public record FileSaveResult(bool IsSuccessful, string? FilePath, string? ErrorMessage = null);
