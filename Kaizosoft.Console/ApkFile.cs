using System.Diagnostics;

namespace Kaizosoft.Console;

public static class ApkFile
{
    public static void AssertAndroidIsConfigured()
    {
        // Check if java exists
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = "-version",
                RedirectStandardError = true, // Java outputs version info to stderr
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            
            process!.WaitForExit();
            // If it successfully ran, Java is present on the PATH environment
        }
        catch (Exception)
        {
            // Thrown if the 'java' executable could not be found
            throw new Exception("Java is not installed or not found in PATH. Please install Java and ensure it's added to your system's PATH environment variable. You can download Java from https://adoptium.net/temurin/releases.");
        }
        
        // Check if apktool.jar and apksigner.jar exists
        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "apktool.jar")))
        {
            throw new FileNotFoundException("apktool.jar not found");
        }
        
        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "apksigner.jar")))
        {
            throw new FileNotFoundException("apksigner.jar not found");
        }
        
        if (Configuration.Android == null)
        {
            throw new Exception("Android configuration is not set. Please provide the necessary keystore information in the configuration.");
        }

        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, Configuration.Android.KeystorePath)))
        {
            throw new FileNotFoundException("Keystore not found");
        }
    }

    public static bool ExtractDirectory(string apkFilePath, string destinationDirectory)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "java",
            ArgumentList =
            {
                "-jar",
                Path.Combine(AppContext.BaseDirectory, "apktool.jar"),
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
                FileName = "java",
                ArgumentList =
                {
                    "-jar",
                    Path.Combine(AppContext.BaseDirectory, "apktool.jar"),
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
                FileName = "java",
                ArgumentList =
                {
                    "-jar",
                    Path.Combine(AppContext.BaseDirectory, "apksigner.jar"),
                    "--ks",
                    Path.Combine(AppContext.BaseDirectory, Configuration.Android!.KeystorePath),
                    "--ksPass",
                    Configuration.Android.KeystorePassword,
                    "--ksAlias",
                    Configuration.Android.KeyAlias,
                    "--ksKeyPass",
                    Configuration.Android.KeyPassword,
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
