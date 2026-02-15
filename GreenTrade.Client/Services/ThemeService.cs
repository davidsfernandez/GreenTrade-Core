using Microsoft.JSInterop;

namespace GreenTrade.Client.Services;

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

    public async Task InitializeAsync()
    {
        var theme = await _jsRuntime.InvokeAsync<string>("appInterop.getTheme");
        _isDarkMode = theme == "dark";
        await ApplyTheme();
    }

    public async Task ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        await ApplyTheme();
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyTheme()
    {
        await _jsRuntime.InvokeVoidAsync("appInterop.setTheme", _isDarkMode);
    }
}
