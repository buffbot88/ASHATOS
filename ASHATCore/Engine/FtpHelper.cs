using System.Net;
using System.Text;
using Abstractions;

#pragma warning disable SYSLIB0014 // FtpWebRequest is obsolete but still recommended for FTP Operations

namespace ASHATCore.Engine;

/// <summary>
/// Helper class for FTP Operations to communicate with local FTP server
/// Uses credentials from ServerConfiguration to connect and manage files
/// </summary>
public class FtpHelper
{
    private readonly string _host;
    private readonly int _port;
    private readonly string? _username;
    private readonly string? _password;

    public FtpHelper(ServerConfiguration config)
    {
        _host = config.FtpHost;
        _port = config.FtpPort;
        _username = config.FtpUsername;
        _password = config.FtpPassword;
    }

    public FtpHelper(string host, int port, string? username, string? password)
    {
        _host = host;
        _port = port;
        _username = username;
        _password = password;
    }

    /// <summary>
    /// Tests connection to the FTP server
    /// </summary>
    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        try
        {
            var url = $"ftp://{_host}:{_port}/";
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Timeout = 5000;

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                request.Credentials = new NetworkCredential(_username, _password);
            }

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            var statusDescription = response.StatusDescription;
            
            return (true, $"Connected to FTP server: {statusDescription}");
        }
        catch (WebException webEx)
        {
            var errorMessage = $"Failed to connect to FTP server at {_host}:{_port}";
            
            if (webEx.Response is FtpWebResponse ftpResponse)
            {
                errorMessage += $" - Status: {ftpResponse.StatusCode} ({ftpResponse.StatusDescription})";
            }
            
            // Add helpful diagnostics
            errorMessage += $"\n  Error: {webEx.Message}";
            
            if (OperatingSystem.IsWindows())
            {
                errorMessage += "\n  Note: On Windows 11, FTP is optional. CMS creation uses Kestrel webserver.";
                errorMessage += "\n  If you need FTP for external file management, ensure FTP server is installed and running.";
            }
            
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to connect to FTP server at {_host}:{_port}: {ex.Message}";
            
            if (OperatingSystem.IsWindows())
            {
                errorMessage += "\n  Note: On Windows 11, FTP is optional for CMS Operations.";
            }
            
            return (false, errorMessage);
        }
    }

    /// <summary>
    /// Uploads a file to the FTP server
    /// </summary>
    public async Task<(bool success, string message)> UploadFileAsync(string localFilePath, string remoteFilePath)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                return (false, $"Local file not found: {localFilePath}");
            }

            var url = $"ftp://{_host}:{_port}/{remoteFilePath.TrimStart('/')}";
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Timeout = 30000;

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                request.Credentials = new NetworkCredential(_username, _password);
            }

            // Read the file
            var fileContents = await File.ReadAllBytesAsync(localFilePath);
            request.ContentLength = fileContents.Length;

            // Upload the file
            using (var requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            return (true, $"File uploaded successfully: {response.StatusDescription}");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to upload file: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads a file from the FTP server
    /// </summary>
    public async Task<(bool success, string message, byte[]? data)> DownloadFileAsync(string remoteFilePath)
    {
        try
        {
            var url = $"ftp://{_host}:{_port}/{remoteFilePath.TrimStart('/')}";
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Timeout = 30000;

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                request.Credentials = new NetworkCredential(_username, _password);
            }

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var memoryStream = new MemoryStream();
            
            await responseStream.CopyToAsync(memoryStream);
            var data = memoryStream.ToArray();
            
            return (true, $"File downloaded successfully: {response.StatusDescription}", data);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to download file: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Lists files in a directory on the FTP server
    /// </summary>
    public async Task<(bool success, string message, List<string>? files)> ListDirectoryAsync(string remotePath = "/")
    {
        try
        {
            var url = $"ftp://{_host}:{_port}/{remotePath.TrimStart('/')}";
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Timeout = 10000;

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                request.Credentials = new NetworkCredential(_username, _password);
            }

            var files = new List<string>();
            
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var reader = new StreamReader(responseStream);
            
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    files.Add(line);
                }
            }
            
            return (true, $"Listed {files.Count} items", files);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to list directory: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Creates a directory on the FTP server
    /// </summary>
    public async Task<(bool success, string message)> CreateDirectoryAsync(string remotePath)
    {
        try
        {
            var url = $"ftp://{_host}:{_port}/{remotePath.TrimStart('/')}";
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Timeout = 10000;

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                request.Credentials = new NetworkCredential(_username, _password);
            }

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            return (true, $"Directory created: {response.StatusDescription}");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to create directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a file from the FTP server
    /// </summary>
    public async Task<(bool success, string message)> DeleteFileAsync(string remoteFilePath)
    {
        try
        {
            var url = $"ftp://{_host}:{_port}/{remoteFilePath.TrimStart('/')}";
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Timeout = 10000;

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                request.Credentials = new NetworkCredential(_username, _password);
            }

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            return (true, $"File deleted: {response.StatusDescription}");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to delete file: {ex.Message}");
        }
    }
}
