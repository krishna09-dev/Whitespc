using Microsoft.JSInterop;

namespace whitespc.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode;

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsDarkMode => _isDarkMode;

    public async Task InitializeAsync(bool isDarkMode)
    {
        _isDarkMode = isDarkMode;
        await ApplyThemeAsync();
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await ApplyThemeAsync();
        OnThemeChanged?.Invoke();
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        _isDarkMode = isDarkMode;
        await ApplyThemeAsync();
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            if (_isDarkMode)
            {
                await _jsRuntime.InvokeVoidAsync("document.documentElement.classList.add", "dark");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("document.documentElement.classList.remove", "dark");
            }
        }
        catch
        {
            // Ignore JS interop errors during prerendering
        }
    }
}
