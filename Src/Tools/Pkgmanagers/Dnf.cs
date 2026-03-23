using orwell.src.types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace orwell.src.tools.pkgmanagers
{
    class Dnf
    {
        public async Task<PackageInfo[]> GetInstalledPackagesAsync()
        {
            var packages = new List<PackageInfo>();

            var process = await GlobalTools.RunProcessAsync("dnf", "list installed");

            if (process.ExitCode != 0)
                return [];

            var lines = process.StdOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            for (int i = 1; i < lines.Length; i++) // Skip the first line which is a header
            {
                var parts = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    packages.Add(new PackageInfo
                    {
                        Id = parts[0],
                        Version = parts[1],
                        PackageManager = "dnf",
                        Source = parts[0]
                    }
                    );
                }
            }
            return packages.ToArray();
        }
    }
}
