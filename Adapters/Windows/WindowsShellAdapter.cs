using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mqtt2Shell.Adapters.Windows
{
    public class WindowsShellAdapter : IShellAdapter
    {
        private readonly IOptions<WindowsShellAdapterSettings> options;
        private readonly ILogger<WindowsShellAdapter> logger;

        public WindowsShellAdapter(IOptions<WindowsShellAdapterSettings> options, ILogger<WindowsShellAdapter> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public void Execute(string commandAndArguments)
        {
            ExecuteCmd(commandAndArguments);
        }

        private void ExecuteCmd(string arguments)
        {
            var settings = options.Value;

            logger.LogInformation($"Executing 'cmd.exe' on '{settings.WorkingDirectory}'. Arguments: '{arguments}'.");

            var startInfo =
                new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    WorkingDirectory = settings.WorkingDirectory,
                    Arguments = "/C " + arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

            int processExitCode;
            var stdError = new StringBuilder();
            var stdOutput = new StringBuilder();
            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, args) => stdOutput.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => stdError.AppendLine(args.Data);

                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();
                processExitCode = process.ExitCode;
            }

            if (processExitCode == 0)
            {
                var result = stdOutput.ToString();
                string message = !string.IsNullOrEmpty(result) ? $":\r\n{result}" : ".";
                logger.LogInformation("Process terminated succesfully" + message);
            }
            else
            {
                var result = stdError.ToString();
                string message = !string.IsNullOrEmpty(result) ? $":\r\n{result}" : "!";
                logger.LogError("Process terminated with errors" + message);
            }
        }
    }
}