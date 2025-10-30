using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Abstractions;

namespace ASHATCore.Engine;

/// <summary>
/// Helper class for HTTP Operations to support AI Framework capabilities
/// Provides methods for downloading files, uploading data, and making HTTP requests
/// </summary>
public class HttpHelper : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly int _timeoutSeconds;

    public HttpHelper(int timeoutSeconds = 30)
    {
        _timeoutSeconds = timeoutSeconds;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    /// <summary>
    /// Downloads a file from a URL
    /// </summary>
    public async Task<(bool success, string message, byte[]? data)> DownloadFileAsync(string url)
    {
        try
        {
            Console.WriteLine($"[HttpHelper] Downloading file from: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", null);
            }

            var data = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"[HttpHelper] ✅ Downloaded {data.Length} bytes");
            
            return (true, $"Downloaded {data.Length} bytes successfully", data);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"HTTP request failed: {ex.Message}", null);
        }
        catch (TaskCanceledException)
        {
            return (false, $"Request timed out after {_timeoutSeconds} seconds", null);
        }
        catch (Exception ex)
        {
            return (false, $"Download failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Downloads a text file from a URL
    /// </summary>
    public async Task<(bool success, string message, string? content)> DownloadTextAsync(string url)
    {
        try
        {
            Console.WriteLine($"[HttpHelper] Downloading text from: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", null);
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[HttpHelper] ✅ Downloaded {content.Length} characters");
            
            return (true, $"Downloaded text successfully", content);
        }
        catch (Exception ex)
        {
            return (false, $"Download failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Posts JSON data to a URL
    /// </summary>
    public async Task<(bool success, string message, string? response)> PostJsonAsync(string url, object data)
    {
        try
        {
            Console.WriteLine($"[HttpHelper] Posting JSON to: {url}");
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", null);
            }

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[HttpHelper] ✅ POST successful");
            
            return (true, "POST successful", responseText);
        }
        catch (Exception ex)
        {
            return (false, $"POST failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Uploads a file using multipart/form-data
    /// </summary>
    public async Task<(bool success, string message, string? response)> UploadFileAsync(
        string url, 
        byte[] fileData, 
        string fileName, 
        string fieldName = "file")
    {
        try
        {
            Console.WriteLine($"[HttpHelper] Uploading file to: {url}");
            
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, fieldName, fileName);
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", null);
            }

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[HttpHelper] ✅ File uploaded successfully");
            
            return (true, "File uploaded successfully", responseText);
        }
        catch (Exception ex)
        {
            return (false, $"Upload failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Makes a GET request to a URL
    /// </summary>
    public async Task<(bool success, string message, string? response, int statusCode)> GetAsync(string url)
    {
        try
        {
            Console.WriteLine($"[HttpHelper] GET request to: {url}");
            
            var response = await _httpClient.GetAsync(url);
            var responseText = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"HTTP {statusCode}: {response.ReasonPhrase}", responseText, statusCode);
            }

            Console.WriteLine($"[HttpHelper] ✅ GET successful (Status: {statusCode})");
            return (true, "GET successful", responseText, statusCode);
        }
        catch (Exception ex)
        {
            return (false, $"GET failed: {ex.Message}", null, 0);
        }
    }

    /// <summary>
    /// Checks if a URL is accessible
    /// </summary>
    public async Task<(bool success, string message, int statusCode)> TestUrlAsync(string url)
    {
        try
        {
            Console.WriteLine($"[HttpHelper] Testing URL: {url}");
            
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(request);
            var statusCode = (int)response.StatusCode;
            
            var isAccessible = response.IsSuccessStatusCode;
            var message = isAccessible 
                ? $"URL is accessible (Status: {statusCode})" 
                : $"URL returned status {statusCode}: {response.ReasonPhrase}";
            
            Console.WriteLine($"[HttpHelper] {(isAccessible ? "✅" : "⚠️")} {message}");
            return (isAccessible, message, statusCode);
        }
        catch (Exception ex)
        {
            return (false, $"URL test failed: {ex.Message}", 0);
        }
    }

    /// <summary>
    /// Downloads a file and saves it to disk
    /// </summary>
    public async Task<(bool success, string message)> DownloadToFileAsync(string url, string localFilePath)
    {
        try
        {
            var result = await DownloadFileAsync(url);
            
            if (!result.success || result.data == null)
            {
                return (false, result.message);
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(localFilePath, result.data);
            Console.WriteLine($"[HttpHelper] ✅ Saved file to: {localFilePath}");
            
            return (true, $"File saved to {localFilePath}");
        }
        catch (Exception ex)
        {
            return (false, $"Save failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets a custom header for all requests
    /// </summary>
    public void SetHeader(string name, string value)
    {
        if (_httpClient.DefaultRequestHeaders.Contains(name))
        {
            _httpClient.DefaultRequestHeaders.Remove(name);
        }
        _httpClient.DefaultRequestHeaders.Add(name, value);
    }

    /// <summary>
    /// Sets authorization header
    /// </summary>
    public void SetAuthorizationHeader(string scheme, string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
