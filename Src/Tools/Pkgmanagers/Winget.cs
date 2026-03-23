using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using orwell.src.types;

namespace orwell.src.tools.pkgmanagers
{
    public class Winget
    {
        private readonly string InstallFlags = "--accept-source-agreements --scope machine -e --silent";
        private readonly string UninstallFlags = "--scope machine -e --silent";

        public async Task<PackageInfo[]> GetInstalledPackagesAsync()
        {
            var packages = new List<PackageInfo>();
            var tempFile = Path.GetTempFileName();
            var process = await GlobalTools.RunProcessAsync("winget", $"export -o \"{tempFile}\" --include-versions");

            if (process.ExitCode != 0)
                return [];

            var jsonRawText = await File.ReadAllTextAsync(tempFile);
            var doc = JsonDocument.Parse(jsonRawText);
            var root = doc.RootElement;
            if (root.TryGetProperty("Sources", out var sources) && sources.GetArrayLength() > 0)
            {
                var source = sources[0];
                if (source.TryGetProperty("Packages", out var pkgs))
                {
                    foreach (var pkg in pkgs.EnumerateArray())
                    {
                        packages.Add(new PackageInfo
                        {
                            Id = pkg.GetProperty("PackageIdentifier").GetString() ?? "unknown",
                            Version = pkg.GetProperty("Version").GetString() ?? "unknown",
                            PackageManager = "winget",
                            Source = "winget"
                        });
                    }
                }
            }

            // Clean up temp file
            try { File.Delete(tempFile); } catch { }

            return packages.ToArray();
        }

        public async Task<InstallStatus[]> InstallPackages(PackageInfo[] packages)
        {
            var results = new List<InstallStatus>();
            foreach (var pkg in packages)
            {
                var result = new InstallStatus { Id = pkg.Id, Version = pkg.Version ?? "latest" };
                try
                {
                    var versionArg = string.IsNullOrEmpty(pkg.Version) ? "" : $"--version \"{pkg.Version}\"";
                    var output = await GlobalTools.RunProcessAsync("winget", $"install --id \"{pkg.Id}\" {versionArg} {this.InstallFlags}");
                    result.Status = EInstallStatus.INSTALLED;
                }
                catch (Exception ex)
                {
                    result.Status = EInstallStatus.ERROR;
                    result.ErrorMessage = ex.Message;
                }
                results.Add(result);
            }
            return results.ToArray();
        }

        public async Task<InstallStatus[]> UninstallPackages(PackageInfo[] packages)
        {
            var results = new List<InstallStatus>();
            foreach (var pkg in packages)
            {
                var result = new InstallStatus { Id = pkg.Id, Version = pkg.Version };
                try
                {
                    var versionArg = string.IsNullOrEmpty(pkg.Version) ? "" : $"--version \"{pkg.Version}\"";
                    var output = await GlobalTools.RunProcessAsync("winget", $"uninstall --id \"{pkg.Id}\" {versionArg} {this.UninstallFlags}");
                    result.Status = EInstallStatus.UNINSTALLED;
                }
                catch (Exception ex)
                {
                    result.Status = EInstallStatus.ERROR;
                    result.ErrorMessage = ex.Message;
                }
                results.Add(result);
            }
            return results.ToArray();
        }
    }
}
