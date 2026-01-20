using System.Buffers.Binary;

namespace Kaizosoft;

public static class BinaryWriterBigEndianExtensions
{
    extension(BinaryWriter writer)
    {
        public void WriteBigEndian(short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteInt16BigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buffer, value);
            writer.Write(buffer);
        }

        public void WriteBigEndian(uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(Half value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteHalfBigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(float value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
            writer.Write(buffer);
        }
        
        public void WriteBigEndian(double value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
            writer.Write(buffer);
        }
    }
}
