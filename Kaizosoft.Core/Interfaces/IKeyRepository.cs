namespace Kaizosoft.Core.Interfaces;

/// <summary>
/// Repository for managing Kairosoft game encryption keys.
/// </summary>
public interface IKeyRepository
{
    /// <summary>
    /// Gets a game key by game name (English or Japanese).
    /// </summary>
    /// <param name="gameName">The English or Japanese name of the game.</param>
    KairosoftKey? GetKeyByGameName(string gameName);
    
    /// <summary>
    /// Loads keys from a CSV file.
    /// </summary>
    /// <param name="filePath">Path to the CSV file containing game keys.</param>
    void LoadKeysFromFile(string filePath);

    /// <summary>
    /// Gets all available keys.
    /// </summary>
    /// <returns>A set of all unique key byte arrays.</returns>
    HashSet<byte[]> Keys { get; }
}
