using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using whitespc.Data;
using whitespc.Models;

namespace whitespc.Services;

public class SecurityService
{
    private readonly JournalDbContext _context;
    private bool _isUnlocked = true;
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationSeconds = 30;

    public event Action? OnLockStateChanged;

    public SecurityService(JournalDbContext context)
    {
        _context = context;
    }

    public bool IsUnlocked => _isUnlocked;

    public async Task<bool> HasPinSetAsync()
    {
        var settings = await GetSettingsAsync();
        return !string.IsNullOrEmpty(settings.PinHash);
    }

    public async Task<bool> HasCompletedOnboardingAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.HasCompletedOnboarding;
    }

    public async Task CompleteOnboardingAsync()
    {
        var settings = await GetSettingsAsync();
        settings.HasCompletedOnboarding = true;
        await _context.SaveChangesAsync();
    }

    public async Task<(bool IsLockedOut, int RemainingSeconds)> CheckLockoutStatusAsync()
    {
        var settings = await GetSettingsAsync();
        
        if (settings.LockoutEndTime.HasValue && settings.LockoutEndTime > DateTime.Now)
        {
            var remaining = (int)(settings.LockoutEndTime.Value - DateTime.Now).TotalSeconds;
            return (true, remaining);
        }
        
        return (false, 0);
    }

    public async Task<(bool Success, int RemainingAttempts, bool IsLockedOut)> ValidatePinAsync(string pin)
    {
        var settings = await GetSettingsAsync();
        
        // Check if currently locked out
        if (settings.LockoutEndTime.HasValue && settings.LockoutEndTime > DateTime.Now)
        {
            return (false, 0, true);
        }

        // Reset lockout if expired
        if (settings.LockoutEndTime.HasValue && settings.LockoutEndTime <= DateTime.Now)
        {
            settings.LockoutEndTime = null;
            settings.FailedPinAttempts = 0;
        }
        
        if (string.IsNullOrEmpty(settings.PinHash))
        {
            _isUnlocked = true;
            return (true, MaxFailedAttempts, false);
        }

        var hash = HashPin(pin);
        var isValid = hash == settings.PinHash;

        if (isValid)
        {
            _isUnlocked = true;
            settings.LastAccessedAt = DateTime.Now;
            settings.LastActivityAt = DateTime.Now;
            settings.FailedPinAttempts = 0;
            settings.LockoutEndTime = null;
            await _context.SaveChangesAsync();
            OnLockStateChanged?.Invoke();
            return (true, MaxFailedAttempts, false);
        }
        else
        {
            settings.FailedPinAttempts++;
            
            if (settings.FailedPinAttempts >= MaxFailedAttempts)
            {
                settings.LockoutEndTime = DateTime.Now.AddSeconds(LockoutDurationSeconds);
                settings.FailedPinAttempts = 0;
                await _context.SaveChangesAsync();
                return (false, 0, true);
            }
            
            await _context.SaveChangesAsync();
            return (false, MaxFailedAttempts - settings.FailedPinAttempts, false);
        }
    }

    public async Task SetPinAsync(string pin)
    {
        var settings = await GetSettingsAsync();
        settings.PinHash = HashPin(pin);
        settings.IsLocked = true;
        settings.FailedPinAttempts = 0;
        settings.LockoutEndTime = null;
        await _context.SaveChangesAsync();
    }

    public async Task RemovePinAsync()
    {
        var settings = await GetSettingsAsync();
        settings.PinHash = null;
        settings.IsLocked = false;
        settings.SecurityQuestion1 = null;
        settings.SecurityAnswer1Hash = null;
        settings.SecurityQuestion2 = null;
        settings.SecurityAnswer2Hash = null;
        settings.SecurityQuestion3 = null;
        settings.SecurityAnswer3Hash = null;
        _isUnlocked = true;
        await _context.SaveChangesAsync();
        OnLockStateChanged?.Invoke();
    }

    // Security Questions for PIN Recovery
    public async Task SetSecurityQuestionsAsync(
        string q1, string a1,
        string q2, string a2,
        string q3, string a3)
    {
        var settings = await GetSettingsAsync();
        settings.SecurityQuestion1 = q1;
        settings.SecurityAnswer1Hash = HashAnswer(a1);
        settings.SecurityQuestion2 = q2;
        settings.SecurityAnswer2Hash = HashAnswer(a2);
        settings.SecurityQuestion3 = q3;
        settings.SecurityAnswer3Hash = HashAnswer(a3);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasSecurityQuestionsAsync()
    {
        var settings = await GetSettingsAsync();
        return !string.IsNullOrEmpty(settings.SecurityQuestion1) &&
               !string.IsNullOrEmpty(settings.SecurityQuestion2) &&
               !string.IsNullOrEmpty(settings.SecurityQuestion3);
    }

    public async Task<(string Q1, string Q2, string Q3)> GetSecurityQuestionsAsync()
    {
        var settings = await GetSettingsAsync();
        return (
            settings.SecurityQuestion1 ?? "",
            settings.SecurityQuestion2 ?? "",
            settings.SecurityQuestion3 ?? ""
        );
    }

    public async Task<bool> ValidateSecurityAnswersAsync(string a1, string a2, string a3)
    {
        var settings = await GetSettingsAsync();
        
        var valid = HashAnswer(a1) == settings.SecurityAnswer1Hash &&
                    HashAnswer(a2) == settings.SecurityAnswer2Hash &&
                    HashAnswer(a3) == settings.SecurityAnswer3Hash;
        
        return valid;
    }

    public async Task ResetPinWithRecoveryAsync(string newPin)
    {
        var settings = await GetSettingsAsync();
        settings.PinHash = HashPin(newPin);
        settings.FailedPinAttempts = 0;
        settings.LockoutEndTime = null;
        _isUnlocked = true;
        await _context.SaveChangesAsync();
        OnLockStateChanged?.Invoke();
    }

    // Activity tracking for auto-lock
    public async Task UpdateActivityAsync()
    {
        var settings = await GetSettingsAsync();
        settings.LastActivityAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ShouldAutoLockAsync()
    {
        var settings = await GetSettingsAsync();
        
        if (string.IsNullOrEmpty(settings.PinHash))
            return false;
            
        if (settings.LockTimeout == LockTimeout.Never)
            return false;
            
        if (settings.LockTimeout == LockTimeout.Always)
            return true;
            
        if (!settings.LastActivityAt.HasValue)
            return true;

        var inactiveMinutes = (DateTime.Now - settings.LastActivityAt.Value).TotalMinutes;
        return inactiveMinutes >= (int)settings.LockTimeout;
    }

    public async Task SetLockTimeoutAsync(LockTimeout timeout)
    {
        var settings = await GetSettingsAsync();
        settings.LockTimeout = timeout;
        await _context.SaveChangesAsync();
    }

    public async Task<LockTimeout> GetLockTimeoutAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.LockTimeout;
    }

    public void Lock()
    {
        _isUnlocked = false;
        OnLockStateChanged?.Invoke();
    }

    public void Unlock()
    {
        _isUnlocked = true;
        OnLockStateChanged?.Invoke();
    }

    public async Task<UserSettings> GetSettingsAsync()
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new UserSettings { Id = 1 };
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        var settings = await GetSettingsAsync();
        settings.IsDarkMode = isDarkMode;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> GetDarkModeAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.IsDarkMode;
    }

    public async Task SetAccentColorAsync(string color)
    {
        var settings = await GetSettingsAsync();
        settings.AccentColor = color;
        await _context.SaveChangesAsync();
    }

    public async Task SetWallpaperAsync(string wallpaper)
    {
        var settings = await GetSettingsAsync();
        settings.Wallpaper = wallpaper;
        await _context.SaveChangesAsync();
    }

    public async Task SetShowDailyMotivationAsync(bool show)
    {
        var settings = await GetSettingsAsync();
        settings.ShowDailyMotivation = show;
        await _context.SaveChangesAsync();
    }

    private static string HashPin(string pin)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin + "whitespc_salt"));
        return Convert.ToBase64String(bytes);
    }

    private static string HashAnswer(string answer)
    {
        // Normalize answer: lowercase and trim
        var normalized = answer.ToLowerInvariant().Trim();
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized + "whitespc_recovery_salt"));
        return Convert.ToBase64String(bytes);
    }
}
