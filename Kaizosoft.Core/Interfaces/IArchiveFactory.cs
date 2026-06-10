namespace Kaizosoft.Core.Interfaces;

/// <summary>
/// Factory for creating KairosoftArchive instances.
/// </summary>
public interface IArchiveFactory
{
    /// <summary>
    /// Creates an archive from encrypted data.
    /// </summary>
    KairosoftArchive CreateFromEncryptedData(byte[] data, byte[] key);
    
    /// <summary>
    /// Creates an archive from unencrypted data.
    /// </summary>
    KairosoftArchive CreateFromData(byte[] data);
}
