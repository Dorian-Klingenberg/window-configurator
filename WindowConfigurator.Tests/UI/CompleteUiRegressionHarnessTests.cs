using System.Diagnostics;

namespace WindowConfigurator.Tests.UI;

public class CompleteUiRegressionHarnessTests
{
    [Fact]
    public void CompleteHarness_VerifiesSuccessAndValidationFailureFlows()
    {
        var solutionRoot = FindSolutionRoot();
        var scriptPath = Path.Combine(solutionRoot, "WindowConfigurator.Tests", "UI", "complete-regression-harness.js");
        Assert.True(File.Exists(scriptPath), $"Harness script not found: {scriptPath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"\"{scriptPath}\"",
            WorkingDirectory = solutionRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        process!.WaitForExit();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        Assert.True(
            process.ExitCode == 0,
            $"UI harness failed with exit code {process.ExitCode}.{Environment.NewLine}stdout:{Environment.NewLine}{stdout}{Environment.NewLine}stderr:{Environment.NewLine}{stderr}");
    }

    private static string FindSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "WindowConfigurator.sln")))
                return current.FullName;

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
