using System.CommandLine;
using System.Globalization;
using Kaizosoft.Core;
using Kaizosoft.Core.Helpers;
using nietras.SeparatedValues;

namespace Kaizosoft.Console;

public static class CommandLine
{
    public static void Run(string[] args)
    {
        Command generateCommand = new("generate", "Generate Kairosoft language file")
        {
            new Argument<DirectoryInfo>("assetsPath")
            {
                HelpName = "Path to the game data folder"
            },
            new Option<string>("--language", "--lang", "-l")
            {
                HelpName = "Language code (e.g., en, jp)",
                Required = true
            },
            new Option<string>("--output", "-o")
            {
                HelpName = "Output directory for generated file"
            }
        };

        generateCommand.SetAction(result =>
        {
            var assetsPath = result.GetRequiredValue<DirectoryInfo>("assetsPath");
            string language = result.GetRequiredValue<string>("--language");
            var output = result.GetValue<DirectoryInfo>("--output") ??
                         new DirectoryInfo(Directory.GetCurrentDirectory());

            try
            {
                using var game = KairosoftGameFactory.CreateWithKeys(assetsPath.FullName, "keys.csv");
                var file = game.CreateTemplateLanguageFile(
                    CultureInfo.GetCultureInfo(language).TwoLetterISOLanguageName);

                output.Create();
                File.WriteAllText(Path.Combine(output.FullName, $"{game.GameName}_{language}.csv"), file.ToCsvString());
            }
            catch (CultureNotFoundException) { throw new Exception($"Invalid language code: {language}"); }
        });

        Command importCommand = new("import", "Import Kairosoft language file")
        {
            new Argument<FileInfo>("languageFile")
            {
                HelpName = "Path to the language CSV file"
            },
            new Argument<DirectoryInfo>("assetsPath")
            {
                HelpName = "Path to the game data folder"
            },
            new Option<string>("--language", "--lang", "-l")
            {
                HelpName = "Language code (e.g., en, jp)"
            },
            new Option<DirectoryInfo>("--output")
            {
                HelpName = "Output directory for modified assets"
            }
        };

        importCommand.SetAction(result =>
        {
            var languageFile = result.GetRequiredValue<FileInfo>("languageFile");
            var assetsPath = result.GetRequiredValue<DirectoryInfo>("assetsPath");
            string? language = result.GetValue<string>("--language");
            var output = result.GetValue<DirectoryInfo>("--output") ?? assetsPath;

            if (language == null)
            {
                string[] fileNameParts = languageFile.Name.Split('_', StringSplitOptions.RemoveEmptyEntries);

                if (fileNameParts.Length < 2)
                {
                    throw new Exception("Language code not specified and could not be inferred from file name.");
                }

                language = Path.GetFileNameWithoutExtension(fileNameParts[^1]);
            }

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
                                : (readRow["Key"].ToString(), readRow["Text"].ToString())).ToArray());

            try
            {
                using var game = KairosoftGameFactory.CreateWithKeys(assetsPath.FullName, "keys.csv");

                game.WriteLanguageFile(CultureInfo.GetCultureInfo(language).TwoLetterISOLanguageName, entries);

                game.SaveLanguageAssets(output.FullName);
            }
            catch (CultureNotFoundException) { throw new Exception($"Invalid language code: {language}"); }
        });

        RootCommand rootCommand = new("Kairosoft Language File Tool")
        {
            Subcommands =
            {
                generateCommand,
                importCommand
            }
        };

        var parseResult = rootCommand.Parse(args);

        parseResult.Invoke();
    }
}
