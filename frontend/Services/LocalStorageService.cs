using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

public class LocalStorageService
{
    private readonly IJSRuntime js;

    public LocalStorageService(IJSRuntime js)
    {
        this.js = js;
    }

    public async Task SaveAsync<T>(string key, T value)
    {
        await js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveAsync(string key)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
