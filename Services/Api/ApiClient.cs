using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using PharmaFind.Models;

namespace PharmaFind.Services.Api;

public class ApiClient
{
    public static string DefaultBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? (DeviceInfo.DeviceType == DeviceType.Virtual ? "http://10.0.2.2:5133" : "http://127.0.0.1:5133")
            : "http://localhost:5133";

    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;
    private string _baseUrl;

    public string BaseUrl => _baseUrl;
    public string? Token { get; set; }

    public ApiClient(string? baseUrl = null)
    {
        _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(5.0)
        };
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10.0) };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public void SetBaseUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            _baseUrl = url.TrimEnd('/');
        }
    }

    private void EnsureAuthHeader()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        EnsureAuthHeader();
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new ServiceUnavailableException($"Erreur API ({(int)response.StatusCode}) : {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct);
            return result ?? throw new ServiceUnavailableException("Réponse vide du serveur.");
        }
        catch (ServiceUnavailableException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("Impossible de joindre le serveur API. Vérifiez votre connexion.");
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken ct = default)
    {
        EnsureAuthHeader();
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _http.PostAsJsonAsync(url, body, _jsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Erreur : {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
            return result ?? throw new ServiceUnavailableException("Réponse vide du serveur.");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ServiceUnavailableException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("Impossible de joindre le serveur API.");
        }
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest body, CancellationToken ct = default)
    {
        EnsureAuthHeader();
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _http.PostAsJsonAsync(url, body, _jsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Erreur : {content}");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ServiceUnavailableException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("Impossible de joindre le serveur API.");
        }
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken ct = default)
    {
        EnsureAuthHeader();
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _http.PutAsJsonAsync(url, body, _jsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Erreur : {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
            return result ?? throw new ServiceUnavailableException("Réponse vide du serveur.");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ServiceUnavailableException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("Impossible de joindre le serveur API.");
        }
    }

    public async Task DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        EnsureAuthHeader();
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _http.DeleteAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Erreur suppression : {content}");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ServiceUnavailableException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("Impossible de joindre le serveur API.");
        }
    }
}
