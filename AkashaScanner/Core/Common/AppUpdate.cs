using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace AkashaScanner.Core
{
    public class AppUpdate
    {
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/xenesty/AkashaScanner/releases/latest";
        private const string AppExecutableName = "AkashaScanner.exe";
        private static readonly string AppExecutable = Path.Combine(Utils.ExecutableDirectory, AppExecutableName);
        private static readonly List<string> RemoveFiles = new()
        {
            AppExecutableName, "AkashaScanner.pdb", "appsettings.json", "*.dll",
        };
        private static readonly List<string> RemoveFolder = new()
        {
            "Resources", "wwwroot", "x64", "x86",
        };

        private readonly ILogger Logger;

        public AppUpdate(ILogger<AppUpdate> logger)
        {
            Logger = logger;
        }

        public Task<string?> Check()
        {
            return GetLatestVersionUrl();
        }

        private async Task<string?> GetLatestVersionUrl()
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("curl", "7.75.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            var resp = await client.GetAsync(LatestReleaseApiUrl);
            if (!resp.IsSuccessStatusCode)
            {
                Logger.LogWarning("Fail to get latest version (status {status}: {reason})", resp.StatusCode, resp.ReasonPhrase);
                return null;
            }
            var body = await resp.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Response>(body)!;
            var tagName = data.tag_name;
            if (!tagName.StartsWith('v'))
            {
                Logger.LogWarning("The latest version has invalid tag name: {tag}", tagName);
                return null;
            }
            if (Utils.AppVersion >= Version.Parse(tagName[1..])) return null;
            foreach (var asset in data.assets)
            {
                var url = asset.browser_download_url;
                if (url.EndsWith(".zip"))
                {
                    return url;
                }
            }
            Logger.LogWarning("Cannot find zip asset");
            return null;
        }

        private async Task<bool> DownloadRelease(string url, string targetDir)
        {
            Logger.LogInformation("Downloading latest release to {target}", targetDir);
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("curl", "7.75.0"));
            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                Logger.LogWarning("Cannot download zip asset  (status {status}: {reason})", resp.StatusCode, resp.ReasonPhrase);
                return false;
            }
            try
            {
                using var stream = await resp.Content.ReadAsStreamAsync();
                using var zip = new ZipArchive(stream);
                zip.ExtractToDirectory(targetDir);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fail to extract to directory");
                return false;
            }
        }

        private static string GetTempDir()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return tempDirectory;
        }

        private static string? FindBaseDir(string dir)
        {
            var exe = Directory.EnumerateFiles(dir, AppExecutableName, SearchOption.AllDirectories).FirstOrDefault();
            if (exe != null)
            {
                return Path.GetDirectoryName(exe);
            }
            return null;
        }

        public async Task StartUpdate(string url)
        {
            var updateDir = GetTempDir();
            var success = await DownloadRelease(url, updateDir);
            if (!success) return;
            var baseDir = FindBaseDir(updateDir);
            if (baseDir == null)
            {
                Logger.LogWarning("Cannot find the directory containing the main executable.");
                return;
            }
            var tempDir = GetTempDir();
            Directory.CreateDirectory(tempDir);
            StringBuilder sb = new();
#if RELEASE
            sb.Append(@"-WindowStyle hidden ");
#endif
            sb.Append(@"-Command ""& {");
            sb.AppendFormat(@"$baseDir = '{0}'; ", baseDir);
            sb.AppendFormat(@"$updateDir = '{0}'; ", updateDir);
            sb.AppendFormat(@"$tempDir = '{0}'; ", tempDir);
            sb.AppendFormat(@"$targetDir = '{0}'; ", Utils.ExecutableDirectory);
            sb.AppendFormat(@"$exe = '{0}'; ", AppExecutable);
#if !RELEASE
            sb.Append(@"Write-Host 'Waiting'$exe' to close'; ");
#endif
            sb.Append(@"while ($True) { ");
            sb.Append(@"Start-Sleep -Seconds 1; ");
            sb.Append(@"$f = New-Object System.IO.FileInfo $exe; ");
            sb.Append(@"try { ");
            sb.Append(@"$s = $f.Open([System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None); ");
            sb.Append(@"if ($s) { ");
            sb.Append(@"$s.Close(); ");
            sb.Append(@"break; ");
            sb.Append(@"} ");
            sb.Append(@"} catch {} ");
            sb.Append(@"} ");
            sb.Append(@"Get-ChildItem $targetDir\* | Where {");
            for (int i = 0; i < RemoveFiles.Count; ++i)
            {
                if (i > 0) sb.Append(@" -or ");
                sb.AppendFormat(@"($_.Name -like '{0}')", RemoveFiles[i]);
            }
            sb.Append(@"} | Move-Item -Destination $tempDir; ");
            foreach (var dir in RemoveFolder)
            {
                sb.AppendFormat(@"Move-Item -Path $targetDir\{0} -Destination $tempDir\{0} -Force -ErrorAction SilentlyContinue; ", dir);
            }
#if !RELEASE
            sb.Append(@"Write-Host 'Moved old files to temporary folder'; ");
#endif
            sb.Append(@"try { ");
            sb.Append(@"Move-Item -Path $baseDir\* -Destination $targetDir -Force; ");
#if !RELEASE
            sb.Append(@"Write-Host 'Moved new files to app folder'; ");
#endif
            sb.Append(@"} catch { ");
            sb.Append(@"Move-Item -Path $tempDir\* -Destination $targetDir -Force; ");
#if !RELEASE
            sb.Append(@"Write-Host 'Reverting update'; ");
#endif
            sb.Append(@"} ");
            sb.Append(@"Remove-Item -Recurse $updateDir -Force; ");
            sb.Append(@"Remove-Item -Recurse $tempDir -Force; ");
            sb.AppendFormat(@"Start-Process $targetDir\{0}; ", AppExecutableName);
            sb.Append(@"}""");

            var args = sb.ToString();
            Logger.LogDebug("command {cmd}", args);
            var startInfo = new ProcessStartInfo()
            {
                FileName = @"powershell.exe",
                Arguments = args,
            };
            var process = new Process()
            {
                StartInfo = startInfo,
            };
            process.Start();
            Environment.Exit(0);
        }

        private record Response
        {
            public string tag_name = string.Empty;
            public List<Asset> assets = default!;
        }

        private class Asset
        {
            public string browser_download_url = default!;
        }
    }
}
