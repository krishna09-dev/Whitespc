using Microsoft.EntityFrameworkCore;
using whitespc.Data;
using whitespc.Models;

namespace whitespc.Services;

public class JournalService
{
    private readonly JournalDbContext _context;

    public JournalService(JournalDbContext context)
    {
        _context = context;
    }

    // ----------------------------
    // Read
    // ----------------------------
    public async Task<JournalEntry?> GetEntryByDateAsync(DateOnly date)
    {
        return await _context.JournalEntries
            .Include(e => e.Tags)
            .Where(e => e.EntryDate == date)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesByDateAsync(DateOnly date)
    {
        return await _context.JournalEntries
            .Include(e => e.Tags)
            .Where(e => e.EntryDate == date)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _context.JournalEntries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<JournalEntry>> GetEntriesAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        List<MoodType>? moods = null,
        List<int>? tagIds = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        bool? isFavorite = null,
        bool? isArchived = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.JournalEntries
            .Include(e => e.Tags)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        if (moods != null && moods.Any())
        {
            query = query.Where(e =>
                moods.Contains(e.PrimaryMood) ||
                (e.SecondaryMood1.HasValue && moods.Contains(e.SecondaryMood1.Value)) ||
                (e.SecondaryMood2.HasValue && moods.Contains(e.SecondaryMood2.Value)));
        }

        if (tagIds != null && tagIds.Any())
            query = query.Where(e => e.Tags.Any(t => tagIds.Contains(t.Id)));

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(e => e.Title.Contains(searchTerm) || e.Content.Contains(searchTerm));

        if (isFavorite.HasValue)
            query = query.Where(e => e.IsFavorite == isFavorite.Value);

        if (isArchived.HasValue)
            query = query.Where(e => e.IsArchived == isArchived.Value);
        else
            query = query.Where(e => !e.IsArchived); // default hide archived

        return await query
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalEntriesCountAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        List<MoodType>? moods = null,
        List<int>? tagIds = null,
        string? searchTerm = null,
        bool? isFavorite = null,
        bool? isArchived = null)
    {
        var query = _context.JournalEntries.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        if (moods != null && moods.Any())
        {
            query = query.Where(e =>
                moods.Contains(e.PrimaryMood) ||
                (e.SecondaryMood1.HasValue && moods.Contains(e.SecondaryMood1.Value)) ||
                (e.SecondaryMood2.HasValue && moods.Contains(e.SecondaryMood2.Value)));
        }

        if (tagIds != null && tagIds.Any())
            query = query.Where(e => e.Tags.Any(t => tagIds.Contains(t.Id)));

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(e => e.Title.Contains(searchTerm) || e.Content.Contains(searchTerm));

        if (isFavorite.HasValue)
            query = query.Where(e => e.IsFavorite == isFavorite.Value);

        if (isArchived.HasValue)
            query = query.Where(e => e.IsArchived == isArchived.Value);
        else
            query = query.Where(e => !e.IsArchived);

        return await query.CountAsync();
    }

    public async Task<List<DateOnly>> GetAllEntryDatesAsync()
    {
        return await _context.JournalEntries
            .Select(e => e.EntryDate)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> HasEntryForDateAsync(DateOnly date)
    {
        return await _context.JournalEntries.AnyAsync(e => e.EntryDate == date);
    }

    // Create (ONE entry per day)
    public async Task<JournalEntry> CreateEntryAsync(JournalEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        // one entry per day check (prevents duplicates)
        var alreadyExists = await _context.JournalEntries
            .AnyAsync(e => e.EntryDate == entry.EntryDate);

        if (alreadyExists)
            throw new InvalidOperationException("An entry already exists for this date. Only one entry per day is allowed.");

        entry.CreatedAt = DateTime.Now;
        entry.UpdatedAt = DateTime.Now;
        entry.UpdateWordCount();

        // Safe tags handling (entry.Tags may be null)
        var tagIds = entry.Tags?.Select(t => t.Id).Distinct().ToList() ?? new List<int>();
        entry.Tags = new List<Tag>();

        if (tagIds.Any())
        {
            var existingTags = await _context.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();

            foreach (var tag in existingTags)
                entry.Tags.Add(tag);
        }

        _context.JournalEntries.Add(entry);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // if unique index exists, this catches race-case duplicates
            throw new InvalidOperationException("An entry already exists for this date. Only one entry per day is allowed.");
        }

        return entry;
    }

    // ----------------------------
    // Update
    // ----------------------------
    public async Task<JournalEntry> UpdateEntryAsync(JournalEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        var existingEntry = await _context.JournalEntries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == entry.Id);

        if (existingEntry == null)
            throw new InvalidOperationException("Entry not found.");


        existingEntry.Title = entry.Title;
        existingEntry.Content = entry.Content;
        existingEntry.PrimaryMood = entry.PrimaryMood;
        existingEntry.SecondaryMood1 = entry.SecondaryMood1;
        existingEntry.SecondaryMood2 = entry.SecondaryMood2;
        existingEntry.UpdatedAt = DateTime.Now;
        existingEntry.UpdateWordCount();

        // Tags
        existingEntry.Tags.Clear();
        var tagIds = entry.Tags?.Select(t => t.Id).Distinct().ToList() ?? new List<int>();

        if (tagIds.Any())
        {
            var existingTags = await _context.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();

            foreach (var tag in existingTags)
                existingEntry.Tags.Add(tag);
        }

        await _context.SaveChangesAsync();
        return existingEntry;
    }

    // ----------------------------
    // Delete
    // ----------------------------
    public async Task DeleteEntryAsync(int id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null) return;

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();
    }

    // ----------------------------
    // Favorites / Pinned
    // ----------------------------
    public async Task<List<JournalEntry>> GetPinnedEntriesAsync()
    {
        return await _context.JournalEntries
            .Include(e => e.Tags)
            .Where(e => e.IsPinned)
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetFavoriteEntriesAsync()
    {
        return await _context.JournalEntries
            .Include(e => e.Tags)
            .Where(e => e.IsFavorite)
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task TogglePinnedAsync(int id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null) return;

        entry.IsPinned = !entry.IsPinned;
        await _context.SaveChangesAsync();
    }

    public async Task ToggleFavoriteAsync(int id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null) return;

        entry.IsFavorite = !entry.IsFavorite;
        await _context.SaveChangesAsync();
    }

    // Pinned first list (optionally favorites only)
    public async Task<List<JournalEntry>> GetEntriesWithPinnedFirstAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        List<MoodType>? moods = null,
        List<int>? tagIds = null,
        string? searchTerm = null,
        bool? favoritesOnly = null,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.JournalEntries
            .Include(e => e.Tags)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        if (moods != null && moods.Any())
        {
            query = query.Where(e =>
                moods.Contains(e.PrimaryMood) ||
                (e.SecondaryMood1.HasValue && moods.Contains(e.SecondaryMood1.Value)) ||
                (e.SecondaryMood2.HasValue && moods.Contains(e.SecondaryMood2.Value)));
        }

        if (tagIds != null && tagIds.Any())
            query = query.Where(e => e.Tags.Any(t => tagIds.Contains(t.Id)));

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(e => e.Title.Contains(searchTerm) || e.Content.Contains(searchTerm));

        if (favoritesOnly == true)
            query = query.Where(e => e.IsFavorite);

        // Hide archived by default (same behavior as GetEntriesAsync)
        query = query.Where(e => !e.IsArchived);

        return await query
            .OrderByDescending(e => e.IsPinned)
            .ThenByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}