using System.Buffers.Binary;

namespace Kaizosoft;

public static class BinaryReaderBigEndianExtensions
{
    extension(BinaryReader binaryReader)
    {
        public short ReadInt16BigEndian() => BinaryPrimitives.ReadInt16BigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(2, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[2]));

        public ushort ReadUInt16BigEndian() => BinaryPrimitives.ReadUInt16BigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(2, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[2]));

        public int ReadInt32BigEndian() => BinaryPrimitives.ReadInt32BigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(4, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[4]));

        public uint ReadUInt32BigEndian() => BinaryPrimitives.ReadUInt32BigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(4, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[4]));

        public long ReadInt64BigEndian() => BinaryPrimitives.ReadInt64BigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(8, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[8]));

        public ulong ReadUInt64BigEndian() => BinaryPrimitives.ReadUInt64BigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(8, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[8]));

        public Half ReadHalfBigEndian() => BinaryPrimitives.ReadHalfBigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(2, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[2]));

        public float ReadSingleBigEndian() => BinaryPrimitives.ReadSingleBigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(4, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[4]));

        public double ReadDoubleBigEndian() => BinaryPrimitives.ReadDoubleBigEndian(
            binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(8, out var readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[8]));

        private ReadOnlySpan<byte> ReadSpan(Span<byte> buffer)
        {
            binaryReader.BaseStream.ReadExactly(buffer);
            return buffer;
        }
    }

    private static bool TryReadSpanUnsafe(this MemoryStream memoryStream, int numBytes, out ReadOnlySpan<byte> readBytes)
    {
        if (memoryStream.TryGetBuffer(out var msBuffer))
        {
            readBytes = msBuffer.AsSpan((int)memoryStream.Position, numBytes);
            memoryStream.Seek(numBytes, SeekOrigin.Current);
            return true;
        }

        readBytes = [];
        return false;
    }
}
