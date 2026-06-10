namespace Kaizosoft.Core.Interfaces;

/// <summary>
/// Repository for managing Kairosoft game encryption keys.
/// </summary>
public interface IKeyRepository
{
    /// <summary>
    /// Gets a game key by game name (English or Japanese).
    /// </summary>
    KairosoftKey? GetKeyByGameName(string gameName);
    
    /// <summary>
    /// Loads keys from a CSV file.
    /// </summary>
    void LoadKeysFromFile(string filePath);

    /// <summary>
    /// Gets all available keys.
    /// </summary>
    HashSet<byte[]> Keys { get; }
}
