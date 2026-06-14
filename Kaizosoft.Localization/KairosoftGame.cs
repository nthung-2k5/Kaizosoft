using System.Globalization;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Kaizosoft.Core.Interfaces;
using Kaizosoft.Localization.Services;

namespace Kaizosoft.Localization;

/// <summary>
/// Represents a Kairosoft game and provides operations for managing language files.
/// </summary>
public sealed class KairosoftGame : IDisposable
{
    private readonly AssetsManager assetManager = new();
    private readonly GameAssetData assetData;

    public string GameName => assetData.GameName;
    public string LanguageAssetsPath => assetData.LanguageAsset.file.path;
    public string ApplicationId => assetData.ApplicationId;
    public string ApplicationVersion => assetData.ApplicationVersion;
    public KairosoftLanguageEntry[] EnglishEntries => assetData.TemplateEntries;

    /// <summary>
    /// Creates a new instance using the provided dependencies.
    /// </summary>
    public KairosoftGame(
        string gameFolder,
        IKeyRepository keyRepository,
        IArchiveFactory archiveFactory)
    {
        if (string.IsNullOrWhiteSpace(gameFolder))
            throw new ArgumentException("Game folder path cannot be null or empty.", nameof(gameFolder));

        var loader = new GameAssetLoader(assetManager, keyRepository, archiveFactory);
        assetData = loader.LoadGameAssets(gameFolder);
    }

    /// <summary>
    /// Creates a template language file for the specified language code.
    /// </summary>
    public KairosoftLanguage CreateTemplateLanguageFile(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language code cannot be null or empty.", nameof(language));

        try
        {
            var culture = CultureInfo.GetCultureInfo(language);
            return new KairosoftLanguageBuilder(culture.NativeName, language, ApplicationId, ApplicationVersion)
                .WithEntries(EnglishEntries)
                .Build();
        }
        catch (CultureNotFoundException ex)
        {
            throw new ArgumentException($"Invalid language code: {language}", nameof(language), ex);
        }
    }

    /// <summary>
    /// Writes a language file to the game archive with the provided entries.
    /// </summary>
    public void WriteLanguageFile(string language, params IReadOnlyList<KairosoftLanguageEntry> entries)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language code cannot be null or empty.", nameof(language));

        if (entries.Count != EnglishEntries.Length || entries.Zip(EnglishEntries, (e, t) => e.Key == t.Key && e.Text.Count == t.Text.Count).Any(match => !match))
            throw new ArgumentException("The number of entries does not match the template entries.", nameof(entries));
        
        try
        {
            var culture = CultureInfo.GetCultureInfo(language);
            var languageFile = new KairosoftLanguageBuilder(
                    culture.NativeName,
                    culture.TwoLetterISOLanguageName,
                    ApplicationId, 
                    ApplicationVersion)
                .WithEntries(entries)
                .Build();

            byte[] fileData = CreateLanguageFileData(languageFile);
            string fileName = GenerateLanguageFileName(languageFile.Language);
            
            assetData.LanguageArchive[fileName] = fileData;
        }
        catch (CultureNotFoundException ex)
        {
            throw new ArgumentException($"Invalid language code: {language}", nameof(language), ex);
        }
    }

    /// <summary>
    /// Saves the modified language assets to the specified output folder.
    /// </summary>
    public void SaveLanguageAssets(string? outputFolder = null)
    {
        if (!string.IsNullOrWhiteSpace(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        else
        {
            outputFolder = Path.GetDirectoryName(LanguageAssetsPath)!;
        }

        assetData.LanguageAsset.baseField["m_Script"].AsByteArray = 
            assetData.LanguageArchive.ToBytes(assetData.GameKey.Key);
        assetData.LanguageAsset.info.SetNewData(assetData.LanguageAsset.baseField);

        string outputPath = Path.Combine(outputFolder, assetData.LanguageAsset.file.name + ".mod");

        using var writer = new AssetsFileWriter(outputPath);
        assetData.LanguageAsset.file.file.Write(writer);
    }

    public void Dispose()
    {
        assetManager.UnloadAll();
    }

    private static byte[] CreateLanguageFileData(KairosoftLanguage languageFile)
    {
        using var stream = new MemoryStream();
        stream.Write(Encoding.UTF8.Preamble);
        stream.Write(Encoding.UTF8.GetBytes(languageFile.ToString()));
        return stream.ToArray();
    }

    private string GenerateLanguageFileName(string language)
    {
        string gameNameWithoutSpaces = string.Join(string.Empty, GameName.Split());
        return $"{gameNameWithoutSpaces}_{language}.csv";
    }
}