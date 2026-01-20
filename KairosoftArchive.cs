using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Kaizosoft;

public sealed class KairosoftArchive : IEnumerable<KairosoftArchive.ArchiveEntry>
{
    private static void CryptBytes(Span<byte> data, ReadOnlySpan<byte> key)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key[i % key.Length];
        }
    }

    public KairosoftArchive()
    {
        files = new OrderedDictionary<string, ArchiveEntry>();
    }

    public KairosoftArchive(byte[] data, byte[]? key = null)
    {
        if (data == null)
        {
            throw new NullReferenceException();
        }

        if (key != null)
        {
            CryptBytes(data, key);
        }

        using var stream = new MemoryStream(data);

        using var reader = new BinaryReader(stream);
        reader.ReadInt32BigEndian();

        int dataLength = reader.ReadInt32BigEndian();
        int filesLength = reader.ReadInt32BigEndian();

        string[] names = new string[filesLength];
        int[] offsets = new int[filesLength];
        int[] sizes = new int[filesLength];

        for (int i = 0; i < filesLength; i++)
        {
            int strLength = reader.ReadInt32BigEndian();
            names[i] = Encoding.UTF8.GetString(reader.ReadBytes(strLength));
        }

        for (int i = 0; i < filesLength; i++)
        {
            offsets[i] = reader.ReadInt32BigEndian();
        }

        for (int i = 0; i < filesLength; i++)
        {
            sizes[i] = reader.ReadInt32BigEndian();
        }

        byte[] flags = reader.ReadBytes(filesLength);
        var archiveData = data.AsSpan((int)stream.Position, dataLength);

        files = new OrderedDictionary<string, ArchiveEntry>();

        for (int i = 0; i < names.Length; i++)
        {
            if ((flags[i] & 1) != 0) throw new InvalidOperationException("Compression is not supported.");

            int fileLength = reader.ReadInt32BigEndian();
            Debug.Assert(sizes[i] == fileLength);

            files.Add(names[i], new ArchiveEntry(names[i], archiveData.Slice(offsets[i] + 4, fileLength).ToArray()));
            reader.BaseStream.Position += fileLength;
        }
    }

    public ArchiveEntry this[string name] => files[name];
    public ArchiveEntry this[int index] => files.GetAt(index).Value;

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

        foreach (var file in fileInfos)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(file.Name);
            writer.WriteBigEndian(nameBytes.Length);
            writer.Write(nameBytes);
        }

        int offset = 0;

        foreach (var file in fileInfos)
        {
            writer.WriteBigEndian(offset);
            offset += file.Size + 4;
        }

        foreach (var file in fileInfos)
        {
            writer.WriteBigEndian(file.Size);
        }

        byte[] flags = new byte[filesLength];
        writer.Write(flags);

        foreach (var file in fileInfos)
        {
            writer.WriteBigEndian(file.Size);
            writer.Write(file.Data);
        }

        byte[] result = stream.ToArray();

        if (key != null) CryptBytes(result, key);

        return result;
    }

    public void AddFile(string name, byte[] data)
    {
        files[name] = new ArchiveEntry(name, data);
    }

    private readonly OrderedDictionary<string, ArchiveEntry> files;

    public record ArchiveEntry(string Name, byte[] Data)
    {
        public int Size => Data.Length;
    }

    public IEnumerator<ArchiveEntry> GetEnumerator() => files.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
