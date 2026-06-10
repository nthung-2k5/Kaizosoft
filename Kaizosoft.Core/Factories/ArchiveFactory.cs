using Kaizosoft.Core.Interfaces;

namespace Kaizosoft.Core.Factories;

/// <summary>
/// Factory for creating KairosoftArchive instances.
/// </summary>
public sealed class ArchiveFactory : IArchiveFactory
{
    public KairosoftArchive CreateFromEncryptedData(byte[] data, byte[] key) { return new KairosoftArchive(data, key); }

    public KairosoftArchive CreateFromData(byte[] data) { return new KairosoftArchive(data); }
}
