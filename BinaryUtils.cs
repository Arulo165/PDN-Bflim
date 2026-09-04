using System;

namespace BflimFileType
{
    public static class BinaryUtils
    {
        public static ushort Read16(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }
    }
}