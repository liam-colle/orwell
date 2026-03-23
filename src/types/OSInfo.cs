using System;
using System.Collections.Generic;
using System.Text;

namespace orwell.src.types
{
    public class OSInfo
    {
        public string OS { get; set; } = "Unknown";
        public string Machine { get; set; } = "Generic";
        public int Uptime { get; set; } = 0;
        public string PackageManager { get; set; } = "unknown";
        public PackageInfo[] Packages { get; set; } = [];
        public WSLInfo? WSLInfo { get; set; }
        public PackageInfo[]? WSLPackages { get; set; }
    }
}
