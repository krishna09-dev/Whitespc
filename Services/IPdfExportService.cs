using whitespc.Models;

namespace whitespc.Services;

public interface IPdfExportService
{
    byte[] ExportToPdf(IReadOnlyList<JournalEntry> entries, string title);
}