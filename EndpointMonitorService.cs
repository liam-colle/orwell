using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace orwell
{
    public class OSInfo
    {
        public string OS { get; set; }
        public string Machine { get; set; }
        public string Uptime { get; set; }
        public string Packages { get; set; }
    }

    public class EndpointMonitorService : BackgroundService
    {
        private readonly ILogger<EndpointMonitorService> _logger;
        private readonly HttpClient _httpClient;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public EndpointMonitorService(ILogger<EndpointMonitorService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var info = await CollectSystemInfoAsync();
                    _logger.LogInformation("System Info: {info}", info);
                    await SendInfoToServerAsync(info, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting system info");
                }
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task SendInfoToServerAsync(OSInfo info, CancellationToken cancellationToken)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { info }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:3000", content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send info to server: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending info to server");
            }
        }

        private async Task<OSInfo> CollectSystemInfoAsync()
        {
            var osInfo = new OSInfo
            {
                OS = RuntimeInformation.OSDescription,
                Machine = Environment.MachineName,
                Uptime = GetUptime(),
                Packages = await GetInstalledPackagesAsync()
            };
            return osInfo;
        }

        private string GetUptime()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                return uptime.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var output = File.ReadAllText("/proc/uptime").Split(' ')[0];
                    var seconds = double.Parse(output);
                    return TimeSpan.FromSeconds(seconds).ToString();
                }
                catch { }
            }
            return "Unknown";
        }

        private async Task<string> GetInstalledPackagesAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await RunProcessAsync("winget", "list");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/usr/bin/apt"))
                    return await RunProcessAsync("apt", "list --installed");
                else if (File.Exists("/usr/bin/dnf"))
                    return await RunProcessAsync("dnf", "list installed");
            }
            return "Unknown or unsupported package manager";
        }

        private async Task<string> RunProcessAsync(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                return string.IsNullOrWhiteSpace(output) ? error : output;
            }
            catch (Exception ex)
            {
                return $"Error running {fileName}: {ex.Message}";
            }
        }
    }
}
