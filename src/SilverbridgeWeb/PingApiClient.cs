namespace SilverbridgeWeb;

public class PingApiClient(HttpClient httpClient)
{
    public async Task<string> PingAsync()
    {
        var response = await httpClient.GetAsync("/api/ping");
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request to ping failed with status code {response.StatusCode}");
        }
        return await response.Content.ReadAsStringAsync();
    }
}
