using System;
using System.Collections.Generic;
using System.Text;

namespace orwell.src.types
{
    public class WSLDistro
    {
        public string Name { get; set; } = "unknown";
        public string State { get; set; } = "unknown";
        public string WslVmVersion { get; set; } = "unknown";

        public string DistroPkgManager { get; set; } = "unknown";
    }
    public class WSLInfo
    {
        public string WslVersion { get; set; } = "unknown";
        public bool IsInstalled { get; set; } = false;
        public WSLDistro[] Distros { get; set; } = [];
    }
}
