using System.Collections;
using System.Diagnostics;
using System.Text;
using Kaizosoft.Core.Helpers;

namespace Kaizosoft.Core;

/// <summary>
/// Represents a Kairosoft game archive that contains multiple files with optional encryption.
/// </summary>
public sealed class KairosoftArchive : IEnumerable<KairosoftArchive.ArchiveEntry>
{
    private readonly OrderedDictionary<string, ArchiveEntry> files;

    /// <summary>
    /// Creates a new archive from binary data with optional decryption.
    /// </summary>
    /// <param name="data">The archive binary data.</param>
    /// <param name="key">Optional encryption key for decryption.</param>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when compression is detected (not supported).</exception>
    public KairosoftArchive(byte[] data, byte[]? key = null)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "Archive data cannot be null.");

        if (key != null)
        {
            CryptBytes(data, key);
        }

        files = ParseArchiveData(data);
    }

    /// <summary>
    /// Gets or sets file data by file name.
    /// </summary>
    public byte[] this[string name]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("File name cannot be null or empty.", nameof(name));
            return files.TryGetValue(name, out var file) ? file.Data : throw new KeyNotFoundException($"File '{name}' not found in archive.");
        }
        set
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("File name cannot be null or empty.", nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value), "File data cannot be null.");

            files[name] = new ArchiveEntry(name, value);
        }
    }

    /// <summary>
    /// Converts the archive to binary format with optional encryption.
    /// </summary>
    /// <param name="key">Optional encryption key.</param>
    /// <returns>The archive as a byte array.</returns>
    public byte[] ToBytes(byte[]? key = null)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.WriteBigEndian(0);
        int dataLength = files.Values.Sum(f => f.Size + 4);
        int filesLength = files.Count;
        writer.WriteBigEndian(dataLength);
        writer.WriteBigEndian(filesLength);

        var fileInfos = files.Values.ToArray();

        // Write file names
        foreach (var file in fileInfos)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(file.Name);
            writer.WriteBigEndian(nameBytes.Length);
            writer.Write(nameBytes);
        }

        // Write offsets
        int offset = 0;
        foreach (var file in fileInfos)
        {
            writer.WriteBigEndian(offset);
            offset += file.Size + 4;
        }

        // Write sizes
        foreach (var file in fileInfos)
        {
            writer.WriteBigEndian(file.Size);
        }

        // Write flags
        byte[] flags = new byte[filesLength];
        writer.Write(flags);

        // Write file data
        foreach (var file in fileInfos)
        {
            writer.WriteBigEndian(file.Size);
            writer.Write(file.Data);
        }

        byte[] result = stream.ToArray();

        if (key != null)
        {
            CryptBytes(result, key);
        }

        return result;
    }

    public IEnumerator<ArchiveEntry> GetEnumerator() => files.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Represents a file entry within the archive.
    /// </summary>
    public record ArchiveEntry(string Name, byte[] Data)
    {
        public int Size => Data.Length;
    }

    private static void CryptBytes(Span<byte> data, ReadOnlySpan<byte> key)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key[i % key.Length];
        }
    }

    private static OrderedDictionary<string, ArchiveEntry> ParseArchiveData(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        reader.ReadInt32BigEndian(); // Skip header
        int dataLength = reader.ReadInt32BigEndian();
        int filesLength = reader.ReadInt32BigEndian();

        string[] names = new string[filesLength];
        int[] offsets = new int[filesLength];
        int[] sizes = new int[filesLength];

        // Read file names
        for (int i = 0; i < filesLength; i++)
        {
            int strLength = reader.ReadInt32BigEndian();
            names[i] = Encoding.UTF8.GetString(reader.ReadBytes(strLength));
        }

        // Read offsets
        for (int i = 0; i < filesLength; i++)
        {
            offsets[i] = reader.ReadInt32BigEndian();
        }

        // Read sizes
        for (int i = 0; i < filesLength; i++)
        {
            sizes[i] = reader.ReadInt32BigEndian();
        }

        byte[] flags = reader.ReadBytes(filesLength);
        var archiveData = data.AsSpan((int)stream.Position, dataLength);

        var files = new OrderedDictionary<string, ArchiveEntry>();

        for (int i = 0; i < names.Length; i++)
        {
            if ((flags[i] & 1) != 0)
            {
                throw new InvalidOperationException(
                    $"Compression detected for file '{names[i]}'. Compression is not supported.");
            }

            int fileLength = reader.ReadInt32BigEndian();
            Trace.Assert(sizes[i] == fileLength, 
                $"Size mismatch for file '{names[i]}': expected {sizes[i]}, got {fileLength}");

            files.Add(names[i], new ArchiveEntry(names[i], 
                archiveData.Slice(offsets[i] + 4, fileLength).ToArray()));
            reader.BaseStream.Position += fileLength;
        }

        return files;
    }
}
