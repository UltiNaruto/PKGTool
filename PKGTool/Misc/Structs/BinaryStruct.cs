using System.IO;

namespace Misc.Structs
{
    public static class BinaryStructExtensions
    {
        public static int Aligned(this int position, int alignment)
        {
            if (position % alignment != 0)
                return position + (alignment - (position % alignment));
            else
                return position;
        }

        public static long Aligned(this long position, long alignment)
        {
            if (position % alignment != 0L)
                return position + (alignment - (position % alignment));
            else
                return position;
        }
    }

    public abstract class BinaryStruct
    {
        public BinaryStruct() { }
        public abstract void import(Stream stream);
        public abstract void export(Stream stream);
        public abstract int StructSize { get; }
    }
}
