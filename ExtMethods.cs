using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PngFix
{
    public static class ExtMethods
    {
        public static int r(this int v) => BinaryPrimitives.ReverseEndianness(v);
        public static uint r(this uint v) => BinaryPrimitives.ReverseEndianness(v);
        public static ulong r(this ulong v) => BinaryPrimitives.ReverseEndianness(v);

        public static uint u32(this byte[] v) => BitConverter.ToUInt32(v);
    }
}
