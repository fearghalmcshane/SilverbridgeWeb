using Microsoft.JSInterop;

namespace SilverbridgeWeb.WebUI.Services.Theme;

internal sealed class ThemePreferenceService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private const string ModulePath = "./js/themePreference.js";
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private IJSObjectReference? _module;
    private DotNetObjectReference<ThemePreferenceService>? _objectReference;

    internal event EventHandler<SystemThemeChangedEventArgs>? SystemThemeChanged;

    internal async Task<ThemePreferenceState> InitializeAsync()
    {
        IJSObjectReference module = await GetModuleAsync();
        _objectReference ??= DotNetObjectReference.Create(this);
        ThemePreferenceResult result = await module.InvokeAsync<ThemePreferenceResult>(
            "initialize",
            _objectReference);

        return ThemePreferenceResolver.Resolve(result.Preference, result.IsDarkMode);
    }

    internal async Task<bool> SetPreferenceAsync(ThemePreference preference)
    {
        IJSObjectReference module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("setPreference", preference.ToString());
    }

    [JSInvokable]
    public Task OnSystemThemeChanged(bool isDarkMode)
    {
        SystemThemeChanged?.Invoke(this, new(isDarkMode));
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                _module = null;
            }
        }

        _objectReference?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        return _module;
    }

    private sealed record ThemePreferenceResult(string Preference, bool IsDarkMode);
}
