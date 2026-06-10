using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Kaizosoft.Core.Constants;
using Kaizosoft.Core.Helpers;
using Kaizosoft.Core.Interfaces;

namespace Kaizosoft.Core.Services;

/// <summary>
/// Service responsible for loading game assets from Unity asset bundles.
/// </summary>
internal sealed class GameAssetLoader(AssetsManager assetManager, IKeyRepository keyRepository, IArchiveFactory archiveFactory)
{
    public GameAssetData LoadGameAssets(string gameFolder)
    {
        if (string.IsNullOrWhiteSpace(gameFolder))
        {
            throw new ArgumentException("Game folder path cannot be null or empty.", nameof(gameFolder));
        }

        if (!Directory.Exists(gameFolder))
        {
            throw new DirectoryNotFoundException($"Game folder not found: {gameFolder}");
        }

        assetManager.LoadClassPackage(GameConstants.CLASS_DATA_PACKAGE_PATH);
        
        var globalGameManagers = assetManager.LoadAssetsFile(
            Path.Combine(gameFolder, GameConstants.GLOBAL_GAME_MANAGERS_FILE_NAME));
        
        assetManager.LoadClassDatabaseFromPackage(globalGameManagers.file.Metadata.UnityVersion);
        
        var playerSettings = assetManager.GetBaseField(
            globalGameManagers, 
            globalGameManagers.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0]);
        
        string gameName = playerSettings["productName"].AsString 
            ?? throw new InvalidOperationException("Game name not found in player settings.");
        
        var gameKey = keyRepository.GetKeyByGameName(gameName) 
            ?? throw new NotSupportedException($"Game '{gameName}' is not supported. No encryption key found.");
        
        var resourceManager = assetManager.GetBaseField(
            globalGameManagers, 
            globalGameManagers.file.GetAssetsOfType(AssetClassID.ResourceManager)[0]);
        
        // Load language archive
        var languageAsset = LoadLanguageAsset(globalGameManagers, resourceManager);
        var languageArchive = archiveFactory.CreateFromEncryptedData(
            languageAsset.baseField["m_Script"].AsByteArray, 
            gameKey.Key);
        
        // Parse language file metadata
        byte[] languageFileData = ExtractLanguageFileData(languageArchive);
        string[] languageFileLines = Encoding.UTF8.GetString(languageFileData)
                                             .ReplaceLineEndings()
                                             .Split(Environment.NewLine);
        
        string applicationId = LanguageFileParser.ParseApplicationId(languageFileLines);
        string applicationVersion = LanguageFileParser.ParseApplicationVersion(languageFileLines);
        
        // Load template entries
        var templateEntries = LoadTemplateEntries(globalGameManagers, resourceManager);
        
        return new GameAssetData(
            gameKey.EnglishName,
            applicationId,
            applicationVersion,
            gameKey,
            languageAsset,
            languageArchive,
            templateEntries);
    }

    private AssetExternal LoadLanguageAsset(AssetsFileInstance ggm, AssetTypeValueField resourceManager)
    {
        var languageContainer = resourceManager["m_Container.Array"]
            .FirstOrDefault(p => p["first"].AsString == GameConstants.DATA_LANGUAGE_RESOURCE_PATH);
        
        if (languageContainer == null)
        {
            throw new InvalidOperationException(
                $"Language resource '{GameConstants.DATA_LANGUAGE_RESOURCE_PATH}' not found in resource manager.");
        }

        return assetManager.GetExtAsset(ggm, languageContainer["second"]);
    }

    private static byte[] ExtractLanguageFileData(KairosoftArchive archive)
    {
        var languageFile = archive.FirstOrDefault(f => f.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
        
        return languageFile == null ? throw new InvalidOperationException("No CSV language file found in archive.") : languageFile.Data;
    }

    private KairosoftLanguageEntry[] LoadTemplateEntries(AssetsFileInstance ggm, AssetTypeValueField resourceManager)
    {
        var templateContainer = resourceManager["m_Container.Array"]
            .FirstOrDefault(p => p["first"].AsString == GameConstants.DATA_LANGUAGE_TEMPLATE_EN_RESOURCE_PATH);
        
        if (templateContainer == null)
        {
            throw new InvalidOperationException(
                $"Template resource '{GameConstants.DATA_LANGUAGE_TEMPLATE_EN_RESOURCE_PATH}' not found in resource manager.");
        }

        var templateAsset = assetManager.GetExtAsset(ggm, templateContainer["second"]);
        byte[] templateData = templateAsset.baseField["m_Script"].AsByteArray;
        
        return LanguageFileParser.ParseTemplateEntries(templateData);
    }
}

/// <summary>
/// Contains all loaded game asset data.
/// </summary>
public sealed record GameAssetData(
    string GameName,
    string ApplicationId,
    string ApplicationVersion,
    KairosoftKey GameKey,
    AssetExternal LanguageAsset,
    KairosoftArchive LanguageArchive,
    KairosoftLanguageEntry[] TemplateEntries);
