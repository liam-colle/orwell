using System;
using System.Collections.Generic;
using System.Text;

namespace orwell.src.types
{
    public class PackageInfo
    {
        public string Id { get; set; } = "unknown";
        public string? Version { get; set; }
        public string? PackageManager { get; set; }
        public string? Source { get; set; }
    }
}
