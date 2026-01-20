using System.CommandLine;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Kaizosoft;
using nietras.SeparatedValues;

internal static class Program
{
    private static readonly List<KairosoftKey> Keys = [];

    static Program()
    {
        using var csv = Sep.Reader().FromFile("keys.csv");
        foreach (var readRow in csv)
        {
            string englishName = readRow[0].ToString();
            string japaneseName = readRow[1].ToString();
            byte[] key = Convert.FromHexString(readRow[2].ToString());
            
            Keys.Add(new KairosoftKey(englishName, japaneseName, key));
        }
    }
    
    public static int Main(string[] args)
    {
        Command generateCommand = new("generate", "Generate Kairosoft language file")
        {
            new Argument<DirectoryInfo>("assetsPath")
            {
                HelpName = "Path to the game data folder",
            },
            new Option<string>("--title", "-t")
            {
                HelpName = "Title of language file",
                Required = true
            },
            new Option<string>("--language", "--lang", "-l")
            {
                HelpName = "Language code (e.g., en, jp)",
                Required = true
            },
            new Option<string>("--output", "-o")
            {
                HelpName = "Output directory for generated file",
            }
        };

        generateCommand.SetAction(result =>
        {
            var assetsPath = result.GetRequiredValue<DirectoryInfo>("assetsPath");
            string title = result.GetRequiredValue<string>("--title");
            string language = result.GetRequiredValue<string>("--language");
            var output = result.GetValue<DirectoryInfo>("--output") ?? new DirectoryInfo(Directory.GetCurrentDirectory());
    
            var manager = new AssetsManager();
            manager.LoadClassPackage("classdata.tpk");
            
            (var assets, string gameName, _, _, var langArchive) = GetGameInformation(manager, assetsPath);
    
            var resourceManager = manager.GetBaseField(assets, assets.file.GetAssetsOfType(AssetClassID.ResourceManager)[0]);
    
            string outputFileName = $"{string.Join(string.Empty, gameName.Split())}_{language}.csv";
    
            string[] langFile = Encoding.UTF8.GetString(langArchive.First(langFile => langFile.Name.EndsWith(".csv")).Data).ReplaceLineEndings().Split(Environment.NewLine);
    
            string appId = langFile.First(line => line.Contains("@appli", StringComparison.Ordinal)).Split(',', 2)[1].Trim('"');
            string appVersion = langFile.First(line => line.Contains("@version", StringComparison.Ordinal)).Split(',', 2)[1].Trim('"');
    
            List<string[]> lines = [
                ["@title", title],
                ["@language", language],
                ["@scale", "auto"],
                ["@appli", appId],
                ["@version", appVersion],
                ["@author", "Kairosoft"]
            ];
    
            var extTemplateAssets = manager.GetExtAsset(assets, resourceManager["m_Container.Array"].First(p => p["first"].AsString == "data/language_pack_template_en")["second"]);
            var templateLanguage = Encoding.UTF8.GetString(extTemplateAssets.baseField["m_Script"].AsByteArray.AsSpan()[Encoding.UTF8.Preamble.Length..]).ReplaceLineEndings().Split(Environment.NewLine).Select(line => line.Split(',', 4));
    
            lines.AddRange(templateLanguage);
    
            output.Create();
            using var writer = Sep.Writer(opt => opt with { WriteHeader = false }).ToFile(Path.Combine(output.FullName, outputFileName));

            foreach (string[] readRow in lines)
            {
                string key = readRow[0];
                string text = readRow[^1];

                if (!string.IsNullOrEmpty(key))
                {
                    key = key.StartsWith('@') ? key : "#" + key.PadLeft(5, '0');
                }
        
                using var writeRow = writer.NewRow();
                writeRow[0].Set(key);
                writeRow[1].Set(text);
            }
        });

        Command importCommand = new("import", "Import Kairosoft language file")
        {
            new Argument<FileInfo>("languageFile")
            {
                HelpName = "Path to the language CSV file",
            },
            new Argument<DirectoryInfo>("assetsPath")
            {
                HelpName = "Path to the game data folder",
            },
            new Option<DirectoryInfo>("--output")
            {
                HelpName = "Output directory for modified assets",
            }
        };

        importCommand.SetAction(result =>
        {
            var languageFile = result.GetRequiredValue<FileInfo>("languageFile");
            var assetsPath = result.GetRequiredValue<DirectoryInfo>("assetsPath");
            var output = result.GetValue<DirectoryInfo>("--output") ?? assetsPath;
    
            var manager = new AssetsManager();
            manager.LoadClassPackage("classdata.tpk");
            
            (_, _, byte[] gameKey, var extLangAssets, var langArchive) = GetGameInformation(manager, assetsPath);
    
            using var reader = Sep.Reader(opt => opt with { HasHeader = false }).FromFile(languageFile.FullName);

            var builder = new StringBuilder();
            foreach (var readRow in reader)
            {
                var key = readRow[0].Span;
                var text = readRow[1].Span;
        
                builder.AppendLine($"{key},{text}");
            }
    
            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            using var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.Preamble);
            stream.Write(Encoding.UTF8.GetBytes(builder.ToString()));
    
            langArchive.AddFile(languageFile.Name,  stream.ToArray());
    
            extLangAssets.baseField["m_Script"].AsByteArray = langArchive.ToBytes(gameKey);
            extLangAssets.info.SetNewData(extLangAssets.baseField);
    
            output.Create();
    
            using AssetsFileWriter writer = new(Path.Combine(output.FullName, extLangAssets.file.name + ".mod"));
            extLangAssets.file.file.Write(writer);
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
        return parseResult.Invoke();
    }

    private static (AssetsFileInstance GlobalGameManagers, string GameName, byte[] GameKey, AssetExternal LanguageAsset, KairosoftArchive Archive) GetGameInformation(AssetsManager manager, DirectoryInfo assetsPath)
    {
        var assets = manager.LoadAssetsFile(Path.Combine(assetsPath.FullName, "globalgamemanagers"));
        manager.LoadClassDatabaseFromPackage(assets.file.Metadata.UnityVersion);
        
        var playerSettings = manager.GetBaseField(assets, assets.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0]);
        var resourceManager = manager.GetBaseField(assets, assets.file.GetAssetsOfType(AssetClassID.ResourceManager)[0]);
        string gameName = playerSettings["productName"].AsString!;
    
        byte[] gameKey = Keys.Find(k => k.EnglishName == gameName || k.JapaneseName == gameName)?.Key ?? [];
        if (gameKey.Length == 0) throw new Exception($"Game name '{gameName}' not found in keys.csv");

        var extLangAssets = manager.GetExtAsset(assets, resourceManager["m_Container.Array"].First(p => p["first"].AsString == "data/language")["second"]);
        var langArchive = new KairosoftArchive(extLangAssets.baseField["m_Script"].AsByteArray, gameKey);
        
        return (assets, gameName, gameKey, extLangAssets, langArchive);
    }
}