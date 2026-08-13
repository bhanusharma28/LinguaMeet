using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LinguaMeet.Web.Services;

public class ApiClientService(IHttpClientFactory factory, IHttpContextAccessor context)
{
    static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    HttpClient Client()
    {
        var c = factory.CreateClient("Api");
        var token = context.HttpContext?.Session.GetString("Token");
        if (token != null)
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    public async Task<(T? Data, string? Error)> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body = null
    )
    {
        var req = new HttpRequestMessage(method, path);
        if (body != null)
            req.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );
        var res = await Client().SendAsync(req);
        var text = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            return (default, ReadMessage(text));
        return (
            string.IsNullOrWhiteSpace(text) ? default : JsonSerializer.Deserialize<T>(text, Json),
            null
        );
    }

    static string ReadMessage(string json)
    {
        try
        {
            return JsonDocument.Parse(json).RootElement.GetProperty("message").GetString()
                ?? "Request failed.";
        }
        catch
        {
            return "Could not complete the request.";
        }
    }
}
