using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace orwell.src.tools
{
    class GlobalTools
    {
        public class ProcessExecResult
        {
            public string StdOut { get; set; } = string.Empty;
            public string StdErr { get; set; } = string.Empty;
            public int ExitCode { get; set; } = -1;
        }
        public static async Task<ProcessExecResult> RunProcessAsync(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                return new ProcessExecResult
                {
                    StdOut = output,
                    StdErr = error,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                return new ProcessExecResult
                {
                    StdOut = "",
                    StdErr = $"Error running {fileName}: {ex.Message}",
                    ExitCode = -1
                };
            }
        }

        public static bool IsWslInstalled()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "wsl",
                    Arguments = "--list --quiet",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                    return false;

                // Check if a distro is installed
                return output.IsWhiteSpace() ? false : true;
            }
            catch
            {
                return false;
            }
        }
    }
}
