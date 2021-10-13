using System.IO;

namespace Misc.Structs
{
    public abstract class BinaryStruct
    {
        public BinaryStruct() { }
        public abstract void import(Stream stream);
        public abstract void export(Stream stream);
        public abstract int StructSize { get; }
    }
}
