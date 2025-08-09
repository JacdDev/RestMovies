using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Movies.SdkConsumer;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _cachedToken = string.Empty;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);

            if (jwt.ValidTo > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }

        await _semaphore.WaitAsync();
        var response = await _httpClient.PostAsJsonAsync("http://localhost:5201//token", new
        {
            userId = "80fd38d3-fa70-4244-9c3c-60381ee73241",
            email = "sample@domain.com",
            customClaims = new Dictionary<string, string>
            {
                { "admin", "true" }
            }
        });

        var newToken = await response.Content.ReadAsStringAsync();
        _cachedToken = newToken;
        _semaphore.Release();

        return _cachedToken;
    }
}