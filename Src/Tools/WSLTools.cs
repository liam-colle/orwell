using System;
using System.Collections.Generic;
using System.Text;

using orwell.src.types;

namespace orwell.src.tools
{
    public class WSLTools
    {
        public static WSLInfo GetWSLInfo()
        {
            var wslInfo = new WSLInfo();
            var wslCheck = GlobalTools.RunProcessAsync("wsl", "--list --verbose").Result;
            if (wslCheck.ExitCode != 0)
                return wslInfo;
            wslInfo.IsInstalled = true;
            var lines = wslCheck.StdOut.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var cleanLine = line
                    .Replace("\0", string.Empty).Trim()
                    .Replace("*", string.Empty).Trim();
                if (cleanLine.StartsWith("NAME"))
                    continue; // Skip header
                var parts = cleanLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var distro = new WSLDistro
                    {
                        Name = parts[0].Replace("\0", string.Empty).Trim(),
                        State = parts[1].Replace("\0", string.Empty).Trim(),
                        WslVmVersion = parts[2].Replace("\0", string.Empty).Trim(),
                        DistroPkgManager = PkgManagerTools.GetPackageManagerName(PkgManagerTools.IdentifyPackageManagerWsl(parts[0].Replace("\0", string.Empty).Trim()))
                    };
                    wslInfo.Distros = wslInfo.Distros.Append(distro).ToArray();
                }
            }
            return wslInfo;
        }
    }
}
