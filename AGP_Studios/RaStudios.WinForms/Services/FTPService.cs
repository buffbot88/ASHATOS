using System;
using System.IO;
using System.Threading.Tasks;
using FluentFTP;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Services
{
    /// <summary>
    /// FTP service for uploading built DLLs to the server.
    /// </summary>
    public class FTPService : IDisposable
    {
        private readonly FTPConfiguration config;
        private AsyncFtpClient? ftpClient;
        private bool isDisposed = false;

        public event EventHandler<string>? OnStatusUpdate;
        public event EventHandler<string>? OnError;

        public FTPService(FTPConfiguration configuration)
        {
            config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Connects to the FTP server.
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                RaiseStatusUpdate($"Connecting to FTP server {config.Host}:{config.Port}...");

                ftpClient = new AsyncFtpClient(
                    config.Host,
                    config.Username,
                    config.Password,
                    config.Port);

                ftpClient.Config.EncryptionMode = config.UseSSL ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
                ftpClient.Config.DataConnectionType = config.PassiveMode ? FtpDataConnectionType.AutoPassive : FtpDataConnectionType.AutoActive;
                ftpClient.Config.ConnectTimeout = config.TimeoutSeconds * 1000;
                ftpClient.Config.DataConnectionConnectTimeout = config.TimeoutSeconds * 1000;

                await ftpClient.Connect();

                RaiseStatusUpdate("Connected to FTP server successfully.");
                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"FTP connection failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Uploads a file to the FTP server.
        /// </summary>
        public async Task<FTPUploadResult> UploadFileAsync(string localPath, string remotePath)
        {
            var result = new FTPUploadResult();

            try
            {
                if (ftpClient == null || !ftpClient.IsConnected)
                {
                    var connected = await ConnectAsync();
                    if (!connected)
                    {
                        result.Success = false;
                        result.Message = "Failed to connect to FTP server";
                        return result;
                    }
                }

                if (!File.Exists(localPath))
                {
                    result.Success = false;
                    result.Message = $"Local file not found: {localPath}";
                    return result;
                }

                var fileName = Path.GetFileName(localPath);
                var fullRemotePath = $"{config.UploadPath}/{fileName}";

                RaiseStatusUpdate($"Uploading {fileName} to {fullRemotePath}...");

                // Create directory if it doesn't exist
                var remoteDir = Path.GetDirectoryName(fullRemotePath)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(remoteDir))
                {
                    await ftpClient.CreateDirectory(remoteDir, true);
                }

                // Upload file
                var status = await ftpClient.UploadFile(localPath, fullRemotePath, FtpRemoteExists.Overwrite);

                if (status == FtpStatus.Success)
                {
                    result.Success = true;
                    result.Message = $"Successfully uploaded {fileName}";
                    result.UploadedFiles = new[] { fileName };
                    RaiseStatusUpdate(result.Message);
                }
                else
                {
                    result.Success = false;
                    result.Message = $"Upload failed with status: {status}";
                    RaiseError(result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Upload error: {ex.Message}";
                RaiseError(result.Message);
                return result;
            }
        }

        /// <summary>
        /// Uploads multiple files to the FTP server.
        /// </summary>
        public async Task<FTPUploadResult> UploadFilesAsync(string[] localPaths)
        {
            var result = new FTPUploadResult();
            var uploadedFiles = new List<string>();

            try
            {
                if (ftpClient == null || !ftpClient.IsConnected)
                {
                    var connected = await ConnectAsync();
                    if (!connected)
                    {
                        result.Success = false;
                        result.Message = "Failed to connect to FTP server";
                        return result;
                    }
                }

                foreach (var localPath in localPaths)
                {
                    var uploadResult = await UploadFileAsync(localPath, config.UploadPath);
                    if (uploadResult.Success)
                    {
                        uploadedFiles.AddRange(uploadResult.UploadedFiles);
                    }
                }

                result.Success = uploadedFiles.Count > 0;
                result.UploadedFiles = uploadedFiles.ToArray();
                result.Message = result.Success
                    ? $"Successfully uploaded {uploadedFiles.Count} file(s)"
                    : "No files were uploaded";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Batch upload error: {ex.Message}";
                RaiseError(result.Message);
                return result;
            }
        }

        /// <summary>
        /// Disconnects from the FTP server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (ftpClient != null && ftpClient.IsConnected)
                {
                    await ftpClient.Disconnect();
                    RaiseStatusUpdate("Disconnected from FTP server.");
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Error disconnecting from FTP: {ex.Message}");
            }
        }

        private void RaiseStatusUpdate(string message)
        {
            OnStatusUpdate?.Invoke(this, message);
        }

        private void RaiseError(string message)
        {
            OnError?.Invoke(this, message);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                DisconnectAsync().GetAwaiter().GetResult();
                ftpClient?.Dispose();
                isDisposed = true;
            }
        }
    }
}
