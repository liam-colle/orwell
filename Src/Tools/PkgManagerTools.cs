using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace orwell.src.tools
{
    public enum EPkgManager
    {
        UNKNOWN = -1,
        WINGET,
        APT,
        DNF
    }

    public class PkgManagerTools
    {
        public static EPkgManager IdentifyPackageManager()
        {
            if (OperatingSystem.IsWindows())
                return EPkgManager.WINGET;
            else if (OperatingSystem.IsLinux())
            {
                // Check for apt
                var aptCheck = GlobalTools.RunProcessAsync("which", "apt").Result;
                if (aptCheck.ExitCode == 0)
                    return EPkgManager.APT;
                // Check for dnf
                var dnfCheck = GlobalTools.RunProcessAsync("which", "dnf").Result;
                if (dnfCheck.ExitCode == 0)
                    return EPkgManager.DNF;
            }
            return EPkgManager.UNKNOWN;
        }

        public static EPkgManager IdentifyPackageManagerWsl()
        {
            if (OperatingSystem.IsWindows())
            {
                if (!GlobalTools.IsWslInstalled())
                    return EPkgManager.UNKNOWN;
                var wslCheck = GlobalTools.RunProcessAsync("wsl", "which apt").Result;
                if (wslCheck.ExitCode == 0)
                    return EPkgManager.APT;
                wslCheck = GlobalTools.RunProcessAsync("wsl", "which dnf").Result;
                if (wslCheck.ExitCode == 0)
                    return EPkgManager.DNF;
            }
            return EPkgManager.UNKNOWN;
        }

        public static EPkgManager IdentifyPackageManagerWsl(string wslVmId)
        {
            if (OperatingSystem.IsWindows())
            {
                if (!GlobalTools.IsWslInstalled())
                    return EPkgManager.UNKNOWN;
                var wslCheck = GlobalTools.RunProcessAsync("wsl", $" -d \"{wslVmId}\" which apt").Result;
                if (wslCheck.ExitCode == 0)
                    return EPkgManager.APT;
                wslCheck = GlobalTools.RunProcessAsync("wsl", $" -d \"{wslVmId}\" which dnf").Result;
                if (wslCheck.ExitCode == 0)
                    return EPkgManager.DNF;
            }
            return EPkgManager.UNKNOWN;
        }

        public static string GetPackageManagerCommand(EPkgManager pkgManager)
        {
            return pkgManager switch
            {
                EPkgManager.WINGET => "winget",
                EPkgManager.APT => "apt",
                EPkgManager.DNF => "dnf",
                _ => throw new NotSupportedException("Unsupported package manager")
            };
        }

        public static string GetPackageManagerName(EPkgManager pkgManager)
        {
            switch (pkgManager)
            {
                case EPkgManager.WINGET:
                    return "Winget";
                case EPkgManager.APT:
                    return "Aptitude";
                case EPkgManager.DNF:
                    return "Dnf";
                default:
                    return "Unknown";
            }
        }
    }
}
