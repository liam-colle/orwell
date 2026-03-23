using System;
using System.Collections.Generic;
using System.Text;

using orwell.src.types;

namespace orwell.src.tools.pkgmanagers
{
    public class Apt
    {
        public async Task<PackageInfo[]> GetInstalledPackagesAsync()
        {
            var packages = new List<PackageInfo>();

            var process = await GlobalTools.RunProcessAsync("apt", "list --installed");

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
                        PackageManager = "apt",
                        Source = parts[0]
                    }
                    );
                }
            }

            return packages.ToArray();
        }
    }
}
