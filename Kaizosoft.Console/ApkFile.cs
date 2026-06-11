using System.Diagnostics;

namespace Kaizosoft.Console;

public static class ApkFile
{
    public static bool ExtractDirectory(string apkFilePath, string destinationDirectory)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(AppContext.BaseDirectory, "apktool.jar"),
            ArgumentList =
            {
                "d",
                apkFilePath,
                "-f",
                "-r",
                "-s",
                "-o",
                destinationDirectory
            },
            UseShellExecute = false, // Required for redirection
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process.Start();
        process.WaitForExit();

        return process.ExitCode == 0;
    }

    public static bool BuildApk(string sourceDirectory, string apkPath)
    {
        using (var process = new Process())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "apktool.jar"),
                ArgumentList =
                {
                    "b",
                    sourceDirectory,
                    "-o",
                    apkPath
                },
                UseShellExecute = false, // Required for redirection
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0) { return false; }
        }

        using (var process = new Process())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "apksigner.jar"),
                ArgumentList =
                {
                    "--ks",
                    Path.Combine(AppContext.BaseDirectory, Configuration.ApkSigner.KeystorePath),
                    "--ksPass",
                    Configuration.ApkSigner.KeystorePassword,
                    "--ksAlias",
                    Configuration.ApkSigner.KeyAlias,
                    "--ksKeyPass",
                    Configuration.ApkSigner.KeyPassword,
                    "-a",
                    apkPath,
                    "--overwrite"
                },
                UseShellExecute = false, // Required for redirection
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();

            // Wait for the process to exit asynchronously
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                File.Delete(apkPath);
                return false;
            }
        }

        return true;
    }
}
