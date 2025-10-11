using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASHATCore.Plugins
{
    public class LegendaryCMSPlugin
    {
        private const string FtpConfigFile = "ftp_config.json";
        private const string WwwRootFolder = "wwwroot";
        private const string CmsModuleFolder = "htdocs";

        public static void Main(string[] args)
        {
            Console.WriteLine("LegendaryCMSPlugin started...");
            var plugin = new LegendaryCMSPlugin();
            plugin.Run();
        }

        public void Run()
        {
            if (!ScanForFtpConfig())
            {
                Console.WriteLine("FTP Configuration file not found. Aborting...");
                return;
            }

            var ftpConfig = LoadFtpConfig();
            if (ftpConfig == null)
            {
                Console.WriteLine("Failed to load FTP Configuration. Aborting...");
                return;
            }

            if (GenerateLegendaryCMSModule())
            {
                Console.WriteLine("LegendaryCMSModule Generated successfully!");
                UploadToServer(ftpConfig, WwwRootFolder, "/wwwroot");
            }
        }

        private bool ScanForFtpConfig()
        {
            if (File.Exists(FtpConfigFile))
            {
                Console.WriteLine($"Found {FtpConfigFile}");
                return true;
            }

            Console.WriteLine($"{FtpConfigFile} not found!");
            return false;
        }

        private FtpConfig? LoadFtpConfig()
        {
            try
            {
                var json = File.ReadAllText(FtpConfigFile);
                var config = JsonSerializer.Deserialize<FtpConfig>(json);
                Console.WriteLine("FTP Configuration loaded successfully!");
                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading FTP Configuration: {ex.Message}");
                return null;
            }
        }

        private bool GenerateLegendaryCMSModule()
        {
            try
            {
                Console.WriteLine("Generating LegendaryCMSModule...");
                var cmsPath = Path.Combine(WwwRootFolder, CmsModuleFolder);

                if (!Directory.Exists(WwwRootFolder))
                {
                    Directory.CreateDirectory(WwwRootFolder);
                }

                if (!Directory.Exists(cmsPath))
                {
                    Directory.CreateDirectory(cmsPath);
                }

                var indexPath = Path.Combine(cmsPath, "index.html");
                File.WriteAllText(indexPath, "<h1>Welcome to LegendaryCMSModule</h1>");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Generate LegendaryCMSModule: {ex.Message}");
                return false;
            }
        }

        private void UploadToServer(FtpConfig ftpConfig, string localPath, string remotePath)
        {
            try
            {
                Console.WriteLine("Uploading files to FTP server...");
                var uploadTasks = new List<Task>();
                foreach (var file in Directory.GetFiles(localPath, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = file.Substring(localPath.Length + 1).Replace("\\", "/");
                    var remoteFilePath = $"{remotePath}/{relativePath}";

                    uploadTasks.Add(UploadFileAsync(ftpConfig, file, remoteFilePath));
                }

                Task.WaitAll(uploadTasks.ToArray());
                Console.WriteLine("Upload completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading files: {ex.Message}");
            }
        }

        private async Task UploadFileAsync(FtpConfig ftpConfig, string localFile, string remoteFile)
        {
            try
            {
                var ftpUri = new Uri($"ftp://{ftpConfig.Host}{remoteFile}");
                // FtpWebRequest is also obsolete, but there is no direct HttpClient support for FTP.
                // For now, use FtpWebRequest, but suppress the warning.
#pragma warning disable SYSLIB0014
                var request = (FtpWebRequest)WebRequest.Create(ftpUri);
#pragma warning restore SYSLIB0014
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpConfig.Username, ftpConfig.Password);

                using (var fileStream = File.OpenRead(localFile))
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await fileStream.CopyToAsync(requestStream);
                }

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                Console.WriteLine($"Uploaded {localFile} -> {remoteFile} ({response.StatusDescription})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload {localFile}: {ex.Message}");
            }
        }
    }

    public class FtpConfig
    {
        public string? Host { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}