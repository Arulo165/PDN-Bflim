using System;

namespace BflimFileType
{
    public static class BinaryUtils
    {
        public static ushort Read16(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        public static uint read32(byte[] data, int start)
        {
            return ((uint)data[start]     << 24) |
                ((uint)data[start + 1] << 16) |
                ((uint)data[start + 2] << 8)  |
                    (uint)data[start + 3];
        }

        public static void write16(byte[] data, int start, ushort value)
        {
            data[start]     = (byte)((value >> 8) & 0xFF);
            data[start + 1] = (byte)(value & 0xFF);
        }

        public static void write32(byte[] data, int start, uint value)
        {
            data[start]     = (byte)((value >> 24) & 0xFF); // Höchstes Byte
            data[start + 1] = (byte)((value >> 16) & 0xFF);
            data[start + 2] = (byte)((value >> 8)  & 0xFF);
            data[start + 3] = (byte)(value         & 0xFF); // Niedrigstes Byte
        }
    }
}