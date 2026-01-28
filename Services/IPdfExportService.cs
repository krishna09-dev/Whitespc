using whitespc.Models;

namespace whitespc.Services;

public interface IPdfExportService
{
    Task<byte[]> ExportToPdfAsync(IReadOnlyList<JournalEntry> entries, string title);
}