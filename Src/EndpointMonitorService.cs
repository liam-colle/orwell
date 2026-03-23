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

// Orwell types
using orwell.src.types;
using orwell.src.tools.pkgmanagers;
using orwell.src.tools;

namespace orwell.src
{
    public class EndpointMonitorService : BackgroundService
    {
        private readonly ILogger<EndpointMonitorService> _Logger;
        private readonly HttpClient _HttpClient;
        private readonly TimeSpan _Interval = TimeSpan.FromSeconds(10);

        // Package Manager Constructors
        private readonly Winget _WingetPkgManager = new Winget();
        private readonly Apt _AptPkgManager = new Apt();
        private readonly Dnf _DnfPkgManager = new Dnf();

        public EndpointMonitorService(ILogger<EndpointMonitorService> logger, HttpClient httpClient)
        {
            _Logger = logger;
            _HttpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var info = await CollectSystemInfoAsync();
                    _Logger.LogInformation("System Info: {info}", info);
                    await SendInfoToServerAsync(info, stoppingToken);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error collecting system info");
                }
                await Task.Delay(_Interval, stoppingToken);
            }
        }

        private async Task SendInfoToServerAsync(OSInfo info, CancellationToken cancellationToken)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { info }), Encoding.UTF8, "application/json");
                var response = await this._HttpClient.PostAsync("http://localhost:3000", content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    this._Logger.LogWarning("Failed to send info to server: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                this._Logger.LogError(ex, "Exception sending info to server");
            }
        }

        private async Task<OSInfo> CollectSystemInfoAsync()
        {
            var osInfo = new OSInfo
            {
                OS = RuntimeInformation.OSDescription,
                Machine = Environment.MachineName,
                Uptime = GetUptime(),
                Packages = await GetInstalledPackagesAsync(),
                WSLInfo = await GetWslInfo()
            };
            return osInfo;
        }

        private int GetUptime()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                return (int) Math.Floor(uptime.TotalSeconds);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var output = File.ReadAllText("/proc/uptime").Split(' ')[0];
                    var seconds = double.Parse(output);
                    return (int) Math.Floor(TimeSpan.FromSeconds(seconds).TotalSeconds);
                }
                catch
                {
                    return -1; // Unable to determine uptime
                }
            }
            return -1;
        }

        private async Task<PackageInfo[]> GetInstalledPackagesAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await this._WingetPkgManager.GetInstalledPackagesAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/usr/bin/apt"))
                    return await this._AptPkgManager.GetInstalledPackagesAsync();
                else if (File.Exists("/usr/bin/dnf"))
                    return await this._DnfPkgManager.GetInstalledPackagesAsync();
            }
            return [];
        }

        private async Task<WSLInfo> GetWslInfo()
        {
            return WSLTools.GetWSLInfo();
        }
    }
}
