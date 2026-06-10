using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using Kaizosoft.Core;
using Kaizosoft.Core.Helpers;
using Microsoft.Extensions.Configuration;
using nietras.SeparatedValues;
using Spectre.Console;

namespace Kaizosoft.Console;

public static class ConsoleInteractive
{
    public const string INPUT_DIRECTORY = "input";
    public const string OUTPUT_DIRECTORY = "output";

    public static DirectoryInfo TemporaryDirectory =>
            field ??= Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.CreateVersion7().ToString()));

    static ConsoleInteractive()
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile(
            "appsettings.json",
            optional: false,
            reloadOnChange: true).Build();

        Configuration.ApkSigner = config.GetSection("ApkSigner").Get<ApkSignerConfiguration>()!;
        Configuration.TranslationSuffix = config.GetValue<string?>("TranslationSuffix", null);

        string? defaultLanguage = config.GetValue<string?>("DefaultLanguage", null);

        if (!string.IsNullOrWhiteSpace(defaultLanguage))
        {
            Configuration.DefaultCulture = CultureInfo.GetCultureInfo(defaultLanguage);
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try { Directory.Delete(TemporaryDirectory.FullName, true); }
            catch
            {
                // Ignore any exceptions during cleanup
            }
        };
    }

    public static void Run()
    {
        AnsiConsole.MarkupLine("[bold blue]Kaizosoft Game Translation Tool[/]");
        AnsiConsole.WriteLine();

        Directory.CreateDirectory(INPUT_DIRECTORY);
        Directory.CreateDirectory(OUTPUT_DIRECTORY);

        KairosoftGame? game = null;

        try
        {
            if (!TryFetchGame(out var gameInfo, out var gamePlatform)) { return; }

            AnsiConsole.WriteLine();

            if (!TryExtractGameArchive(gameInfo, out var extractionDir)) { return; }

            AnsiConsole.WriteLine();

            if (!TryLoadGame(extractionDir, gamePlatform.Value, out game)) { return; }

            AnsiConsole.WriteLine();

            var languageFile = FetchTranslationFile();

            if (languageFile != null) // Apply translation
            {
                AnsiConsole.MarkupLine($"[green]:check_mark: Translation file detected: {languageFile.Name}.[/]");

                string[] fileNameParts = languageFile.Name.Split('_', StringSplitOptions.RemoveEmptyEntries);

                CultureInfo culture;

                try
                {
                    string language;

                    if (fileNameParts.Length < 2)
                    {
                        if (Configuration.DefaultCulture != null)
                        {
                            language = Configuration.DefaultCulture.TwoLetterISOLanguageName;
                            AnsiConsole.MarkupLine(
                                $"[yellow]:warning: No language code found in file name. Using default language code from configuration: {language}[/]");
                        }
                        else
                        {
                            language = AnsiConsole.Ask<string>(
                                "Language code could not be determined from file name. Please enter the ISO language code (e.g., 'vi' for Vietnamese):");
                        }

                        if (string.IsNullOrWhiteSpace(language))
                        {
                            AnsiConsole.MarkupLine("[red]:x: No language code provided. Aborting.[/]");
                            return;
                        }
                    }
                    else { language = Path.GetFileNameWithoutExtension(fileNameParts[^1]); }

                    culture = CultureInfo.GetCultureInfo(language);
                }
                catch (CultureNotFoundException ex)
                {
                    AnsiConsole.MarkupLine(
                        $"[red]:x: Invalid language code detected: {ex.InvalidCultureName}. Aborting.[/]");
                    return;
                }

                if (Configuration.DefaultCulture == null)
                {
                    AnsiConsole.MarkupLine(
                        $"[green]:check_mark: Detected language code: {culture.TwoLetterISOLanguageName} ({culture.NativeName})[/]");
                    AnsiConsole.WriteLine();
                }

                if (!PromptToContinue(gameInfo, languageFile, gamePlatform.Value, culture)) { return; }

                AnsiConsole.WriteLine();

                if (!ApplyTranslation(game, languageFile, culture)) { return; }

                if (!RepackGame(gameInfo, extractionDir, gamePlatform.Value, out var repackedFile)) { return; }

                AnsiConsole.MarkupLine(
                    $"[green]:check_mark: Translation process completed successfully! Repacked file located at:[/] {repackedFile.Name}");
            }
            else // Extract translation
            {
                CultureInfo culture;

                try
                {
                    string language = AnsiConsole.Ask(
                        "Enter the ISO language code to the language file (e.g., 'vi' for Vietnamese). Press Enter to use default language",
                        defaultValue: Configuration.DefaultCulture?.TwoLetterISOLanguageName!);

                    if (string.IsNullOrWhiteSpace(language))
                    {
                        AnsiConsole.MarkupLine("[red]:x: No language code provided. Aborting.[/]");
                        return;
                    }

                    culture = CultureInfo.GetCultureInfo(language);
                }
                catch (CultureNotFoundException ex)
                {
                    AnsiConsole.MarkupLine(
                        $"[red]:x: Invalid language code detected: {ex.InvalidCultureName}. Aborting.[/]");
                    return;
                }

                AnsiConsole.MarkupLine(
                    $"[green]:check_mark: Detected language code: {culture.TwoLetterISOLanguageName} ({culture.NativeName})[/]");
                AnsiConsole.WriteLine();

                if (!PromptToContinue(gameInfo, languageFile, gamePlatform.Value, culture)) { return; }

                AnsiConsole.WriteLine();

                ExtractTranslation(gameInfo, game, culture);
            }
        }
        finally { game?.Dispose(); }
    }

    public enum GamePlatform
    {
        Windows,
        Android
    }

    private static bool TryFetchGame([NotNullWhen(true)] out FileInfo? gameInfo,
                                     [NotNullWhen(true)] out GamePlatform? gamePlatform)
    {
        var dirInfo = new DirectoryInfo(INPUT_DIRECTORY);
        var files = dirInfo.EnumerateFiles().Where(f =>
                f.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
                f.Extension.Equals(".apk", StringComparison.OrdinalIgnoreCase)).ToArray();

        switch (files.Length)
        {
            case 1:
                AnsiConsole.MarkupLine($"[green]:check_mark: Found game file:[/] {files[0].Name}");
                gameInfo = files[0];
                gamePlatform = files[0].Extension.Equals(".apk", StringComparison.OrdinalIgnoreCase)
                        ? GamePlatform.Android
                        : GamePlatform.Windows;
                return true;
            case > 1:
                AnsiConsole.MarkupLine(
                    "[yellow]:warning: Multiple game files found in input directory. Please ensure only one game file is present.[/]");
                break;
            default:
                AnsiConsole.MarkupLine(
                    "[red]:x: No game files found in input directory. Please add a .zip or .apk file to proceed.[/]");
                break;
        }

        gameInfo = null;
        gamePlatform = null;
        return false;
    }

    private static FileInfo? FetchTranslationFile()
    {
        var dirInfo = new DirectoryInfo(INPUT_DIRECTORY);
        var files = dirInfo.EnumerateFiles().Where(f => f.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                .ToArray();

        switch (files.Length)
        {
            case 1:
                return files[0];
            case > 1:
                AnsiConsole.MarkupLine(
                    "[yellow]:warning: Multiple translation files found in input directory. Please ensure only one translation file is present.[/]");
                break;
        }

        return null;
    }

    private static bool TryExtractGameArchive(FileInfo archiveFile,
                                              [NotNullWhen(true)] out DirectoryInfo? extractionDir)
    {
        if (archiveFile.Extension.Equals(".apk", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("Detected APK file. Extracting using apktool...");
        }
        else
        {
            Trace.Assert(archiveFile.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase));
            AnsiConsole.MarkupLine("Detected ZIP file. Extracting using built-in ZIP extractor...");
        }

        var dir = TemporaryDirectory.CreateSubdirectory(Guid.CreateVersion7().ToString());
        bool ok = AnsiConsole.Status().Start(
            "Extracting game archive...",
            _ =>
            {
                if (archiveFile.Extension.Equals(".apk", StringComparison.OrdinalIgnoreCase))
                {
                    return ApkFile.ExtractDirectory(archiveFile.FullName, dir.FullName);
                }

                try
                {
                    ZipFile.ExtractToDirectory(archiveFile.FullName, dir.FullName);
                    return true;
                }
                catch { return false; }
            });

        if (!ok)
        {
            AnsiConsole.MarkupLine("[red]:x: Failed to extract game archive.[/]");
            extractionDir = null;
            return false;
        }

        AnsiConsole.MarkupLine("[green]:check_mark: Successfully extracted archive.[/]");
        extractionDir = dir;
        return true;
    }

    private static bool TryLoadGame(DirectoryInfo extractionDir, GamePlatform platform,
                                    [NotNullWhen(true)] out KairosoftGame? game)
    {
        try
        {
            if (platform == GamePlatform.Android)
            {
                // For Android, the data directory is under assets/bin/Data
                string dataDir = Path.Combine(extractionDir.FullName, "assets", "bin", "Data");

                if (!Directory.Exists(dataDir))
                {
                    throw new DirectoryNotFoundException("Could not find Data directory in extracted APK files.");
                }

                extractionDir = new DirectoryInfo(dataDir);
            }
            else
            {
                // For desktop, find the _Data folder
                var dataDir = extractionDir.GetDirectories("*_Data", SearchOption.AllDirectories).FirstOrDefault();

                extractionDir = dataDir ??
                                throw new DirectoryNotFoundException(
                                    "Could not find _Data directory in extracted files.");
            }

            game = KairosoftGameFactory.CreateWithKeys(extractionDir.FullName, "keys.csv");

            AnsiConsole.MarkupLine($"[green]:check_mark: Loaded game:[/] {game.GameName} ({platform})");
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]:x: Failed to load game from extracted files: {ex.Message}[/]");

            game = null;
            return false;
        }
    }

    private static bool ApplyTranslation(KairosoftGame game, FileInfo languageFile, CultureInfo culture)
    {
        using var csv = Sep.Reader(opt => opt with
        {
            Unescape = true,
            DisableQuotesParsing = false
        }).FromFile(languageFile.FullName);

        var entries = LanguageFileParser.ParseTemplateEntries(
            csv.Enumerate(readRow =>
                            readRow.ColCount < 2
                                    ? throw new Exception(
                                        "Invalid CSV format. Each row must have at least two columns: Key and Text.")
                                    : (readRow["Key"].ToString(), readRow["Text"].ToString().ReplaceLineEndings("")))
                    .ToArray());

        game.WriteLanguageFile(culture.TwoLetterISOLanguageName, entries);
        game.SaveLanguageAssets();
        game.Dispose();

        File.Move(game.LanguageAssetsPath + ".mod", game.LanguageAssetsPath, true);

        AnsiConsole.MarkupLine($"[green]:check_mark: Successfully applied {entries.Length} translation entries.[/]");

        return true;
    }

    private static bool RepackGame(FileInfo originalGameInfo, DirectoryInfo extractionDir, GamePlatform platform,
                                   [NotNullWhen(true)] out FileInfo? repackedFile)
    {
        repackedFile = AnsiConsole.Status().Start(
            "Repacking game archive...",
            ctx =>
            {
                if (platform == GamePlatform.Android)
                {
                    ctx.Status("Repacking APK using apktool and apksigner...");

                    string outputApkPath = Path.Combine(
                        OUTPUT_DIRECTORY,
                        Path.GetFileNameWithoutExtension(originalGameInfo.Name) + Configuration.TranslationSuffix +
                        ".apk");
                    return ApkFile.BuildApk(extractionDir.FullName, outputApkPath) ? new FileInfo(outputApkPath) : null;
                }

                ctx.Status("Repacking ZIP using built-in ZIP compressor...");

                string outputZipPath = Path.Combine(
                    OUTPUT_DIRECTORY,
                    Path.GetFileNameWithoutExtension(originalGameInfo.Name) + Configuration.TranslationSuffix + ".zip");

                if (File.Exists(outputZipPath)) { File.Delete(outputZipPath); }

                ZipFile.CreateFromDirectory(extractionDir.FullName, outputZipPath);

                return new FileInfo(outputZipPath);
            });

        if (repackedFile == null)
        {
            AnsiConsole.MarkupLine("[red]:x: Failed to repackage game archive.[/]");
            return false;
        }

        return true;
    }

    private static void ExtractTranslation(FileInfo originalGameInfo, KairosoftGame game, CultureInfo culture)
    {
        var file = AnsiConsole.Status().Start(
            "Extracting translations...",
            _ =>
            {
                var file = game.CreateTemplateLanguageFile(culture.TwoLetterISOLanguageName);
                AnsiConsole.MarkupLine($"[green]:check_mark: Extracted {file.Entries.Count} translation entries.[/]");
                return file;
            });

        AnsiConsole.Status().Start(
            "Saving translations to CSV...",
            _ =>
            {
                string outputFileName =
                        $"{Path.GetFileNameWithoutExtension(originalGameInfo.Name)}_{culture.TwoLetterISOLanguageName}.csv";

                File.WriteAllText(Path.Combine(OUTPUT_DIRECTORY, outputFileName), file.ToCsvString());

                AnsiConsole.MarkupLine($"[green]:check_mark: Saved translation entries to:[/] {outputFileName}");
            });
    }

    private static bool PromptToContinue(FileInfo originalGameInfo, FileInfo? languageFile, GamePlatform platform,
                                         CultureInfo culture)
    {
        var panel = new Panel(
                    $"- Game File: [blue]{originalGameInfo.Name}[/]\n" + $"- Platform: [blue]{platform}[/]\n" +
                    (languageFile != null ? $"- Translation File: [blue]{languageFile.Name}[/]\n" : string.Empty) +
                    $"- Language: [blue]{culture.TwoLetterISOLanguageName} ({culture.NativeName})[/]\n" +
                    (languageFile != null
                            ? "- Action: Apply translation and repack game archive"
                            : "- Action: Extract translation file from game archive"))
                .Header("Summary of actions to be performed");

        AnsiConsole.Write(panel);
        return AnsiConsole.Confirm("Do you want to proceed?");
    }
}
