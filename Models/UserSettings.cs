using System.ComponentModel.DataAnnotations;

namespace whitespc.Models;

public enum LockTimeout
{
    Always = 0,
    OneMinute = 1,
    FiveMinutes = 5,
    FifteenMinutes = 15,
    ThirtyMinutes = 30,
    Never = -1
}

public class UserSettings
{
    [Key]
    public int Id { get; set; }

    public string? PinHash { get; set; }

    public bool IsDarkMode { get; set; } = false;

    public bool IsLocked { get; set; } = false;

    public DateTime? LastAccessedAt { get; set; }

    // PIN Auto-Timeout
    public LockTimeout LockTimeout { get; set; } = LockTimeout.FiveMinutes;
    public DateTime? LastActivityAt { get; set; }

    // PIN Recovery - Security Questions (hashed answers)
    public string? SecurityQuestion1 { get; set; }
    public string? SecurityAnswer1Hash { get; set; }
    public string? SecurityQuestion2 { get; set; }
    public string? SecurityAnswer2Hash { get; set; }
    public string? SecurityQuestion3 { get; set; }
    public string? SecurityAnswer3Hash { get; set; }

    // Recovery Attempt Limit
    public int FailedPinAttempts { get; set; } = 0;
    public DateTime? LockoutEndTime { get; set; }

    // Onboarding
    public bool HasCompletedOnboarding { get; set; } = false;

    // Theme & Personalization
    public string AccentColor { get; set; } = "violet"; // violet, blue, green, rose, amber, cyan
    public string Wallpaper { get; set; } = "none"; // none, gradient1, gradient2, pattern1, etc.

    // Daily Motivation
    public bool ShowDailyMotivation { get; set; } = true;
}
