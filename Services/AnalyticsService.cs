using Microsoft.EntityFrameworkCore;
using whitespc.Data;
using whitespc.Models;

namespace whitespc.Services;

public class AnalyticsService
{
    private readonly JournalDbContext _context;

    public AnalyticsService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<MoodDistribution> GetMoodDistributionAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.JournalEntries.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();

        if (!entries.Any())
            return new MoodDistribution();

        var allMoods = entries.SelectMany(e => e.AllMoods).ToList();
        var total = allMoods.Count;

        return new MoodDistribution
        {
            PositiveCount = allMoods.Count(m => m.GetCategory() == MoodCategory.Positive),
            NeutralCount = allMoods.Count(m => m.GetCategory() == MoodCategory.Neutral),
            NegativeCount = allMoods.Count(m => m.GetCategory() == MoodCategory.Negative),
            TotalCount = total,
            MoodCounts = allMoods.GroupBy(m => m)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<MoodType?> GetMostFrequentMoodAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var distribution = await GetMoodDistributionAsync(startDate, endDate);
        
        if (distribution.MoodCounts == null || !distribution.MoodCounts.Any())
            return null;

        return distribution.MoodCounts.OrderByDescending(m => m.Value).First().Key;
    }

    public async Task<StreakInfo> GetStreakInfoAsync()
    {
        var entryDates = await _context.JournalEntries
            .Select(e => e.EntryDate)
            .OrderBy(d => d)
            .ToListAsync();

        if (!entryDates.Any())
            return new StreakInfo();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentStreak = 0;
        var longestStreak = 0;
        var tempStreak = 1;

        // Calculate longest streak
        for (int i = 1; i < entryDates.Count; i++)
        {
            if (entryDates[i].DayNumber - entryDates[i - 1].DayNumber == 1)
            {
                tempStreak++;
            }
            else
            {
                longestStreak = Math.Max(longestStreak, tempStreak);
                tempStreak = 1;
            }
        }
        longestStreak = Math.Max(longestStreak, tempStreak);

        // Calculate current streak
        var checkDate = today;
        while (entryDates.Contains(checkDate))
        {
            currentStreak++;
            checkDate = checkDate.AddDays(-1);
        }

        // If no entry today, check if yesterday had entry for streak continuation
        if (currentStreak == 0 && entryDates.Contains(today.AddDays(-1)))
        {
            checkDate = today.AddDays(-1);
            while (entryDates.Contains(checkDate))
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
        }

        return new StreakInfo
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            TotalEntries = entryDates.Count
        };
    }


    public async Task<List<DateOnly>> GetMissedDaysAsync(DateOnly startDate, DateOnly endDate)
    {
        var entryDates = await _context.JournalEntries
            .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
            .Select(e => e.EntryDate)
            .Distinct()
            .ToListAsync();

        var missedDays = new List<DateOnly>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            if (!entryDates.Contains(currentDate))
                missedDays.Add(currentDate);
            currentDate = currentDate.AddDays(1);
        }

        return missedDays;
    }

    public async Task<List<DateOnly>> GetEntryDatesAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.JournalEntries
            .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
            .Select(e => e.EntryDate)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }


    public async Task<Dictionary<string, int>> GetTagUsageAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.JournalEntries
            .Include(e => e.Tags)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();

        return entries
            .SelectMany(e => e.Tags)
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public async Task<List<WordCountTrend>> GetWordCountTrendsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        // Always show last 7 days for word count trend
        var end = endDate ?? DateOnly.FromDateTime(DateTime.Today);
        var start = end.AddDays(-6); // 7 days including today
        
        var query = _context.JournalEntries
            .Where(e => e.EntryDate >= start && e.EntryDate <= end);

        var entries = await query.ToListAsync();
        
        // Group by date and sum word counts for that date
        var entriesByDate = entries
            .GroupBy(e => e.EntryDate)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.WordCount));
        
        // Generate all 7 days with 0 for missing days
        var result = new List<WordCountTrend>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            result.Add(new WordCountTrend
            {
                Date = date,
                WordCount = entriesByDate.TryGetValue(date, out var count) ? count : 0
            });
        }
        
        return result;
    }

    public async Task<double> GetAverageWordCountAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.JournalEntries.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();
        
        if (!entries.Any())
            return 0;

        return entries.Average(e => e.WordCount);
    }
}

public class MoodDistribution
{
    public int PositiveCount { get; set; }
    public int NeutralCount { get; set; }
    public int NegativeCount { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<MoodType, int>? MoodCounts { get; set; }

    public double PositivePercentage => TotalCount > 0 ? (double)PositiveCount / TotalCount * 100 : 0;
    public double NeutralPercentage => TotalCount > 0 ? (double)NeutralCount / TotalCount * 100 : 0;
    public double NegativePercentage => TotalCount > 0 ? (double)NegativeCount / TotalCount * 100 : 0;
}

public class StreakInfo
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalEntries { get; set; }
}

public class WordCountTrend
{
    public DateOnly Date { get; set; }
    public int WordCount { get; set; }
}
