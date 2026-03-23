using System;
using System.Collections.Generic;
using System.Text;

namespace orwell.src.types
{
    public class InstallStatus
    {
        public string Id { get; set; } = "unknown";
        public string Version { get; set; } = "unknown";
        public EInstallStatus Status { get; set; } = EInstallStatus.UNKNOWN;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public enum EInstallStatus
    {
        UNKNOWN = -1,
        INSTALLED,
        UNINSTALLED,
        ERROR
    }
}
