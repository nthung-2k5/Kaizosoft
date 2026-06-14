using Kaizosoft.Core.Factories;
using Kaizosoft.Core.Interfaces;
using Kaizosoft.Core.Repositories;

namespace Kaizosoft.Localization;

/// <summary>
/// Composition root for dependency injection setup.
/// Provides factory methods to create properly configured KairosoftGame instances.
/// </summary>
public static class KairosoftGameFactory
{
    /// <summary>
    /// Creates a KairosoftGame instance with all dependencies properly configured.
    /// </summary>
    /// <param name="gameFolder">Path to the game data folder.</param>
    /// <param name="keyRepository">Optional custom key repository. If null, uses a new instance.</param>
    /// <returns>A configured KairosoftGame instance.</returns>
    public static KairosoftGame Create(string gameFolder, IKeyRepository? keyRepository = null)
    {
        var repository = keyRepository ?? new KeyRepository();
        var archiveFactory = new ArchiveFactory();

        return new KairosoftGame(
            gameFolder,
            repository,
            archiveFactory);
    }

    /// <summary>
    /// Creates a KairosoftGame instance with a preloaded key repository from CSV file.
    /// </summary>
    /// <param name="gameFolder">Path to the game data folder.</param>
    /// <param name="keysFilePath">Path to the CSV file containing game keys.</param>
    /// <returns>A configured KairosoftGame instance.</returns>
    public static KairosoftGame CreateWithKeys(string gameFolder, string keysFilePath)
    {
        var keyRepository = new KeyRepository();
        keyRepository.LoadKeysFromFile(keysFilePath);

        return Create(gameFolder, keyRepository);
    }
}
