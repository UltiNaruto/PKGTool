using System.Linq;
using System.Text;

namespace System.IO
{
    public class BinaryReaderLE : BinaryReader
    {
        public BinaryReaderLE(Stream input) : base(input)
        {
        }

        public BinaryReaderLE(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public BinaryReaderLE(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override Stream BaseStream => base.BaseStream;

        public override void Close()
        {
            base.Close();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int PeekChar()
        {
            return base.PeekChar();
        }

        public override int Read()
        {
            return base.Read();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return base.Read(buffer, index, count);
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            return base.Read(buffer, index, count);
        }

        public override bool ReadBoolean()
        {
            return base.ReadBoolean();
        }

        public override byte[] ReadBytes(int count)
        {
            return base.ReadBytes(count);
        }

        public override char ReadChar()
        {
            return (char)ReadByte();
        }

        public char ReadWChar()
        {
            return Encoding.Unicode.GetString(ReadBytes(2)).First();
        }

        public override char[] ReadChars(int count)
        {
            String str = "";
            for (int i = 0; i < count; i++) str += ReadChar();
            return str.ToCharArray();
        }

        public char[] ReadWChars(int count)
        {
            String str = "";
            for (int i = 0; i < count; i++) str += ReadWChar();
            return str.ToCharArray();
        }

        public override decimal ReadDecimal()
        {
            throw new NotImplementedException();
        }

        public override sbyte ReadSByte()
        {
            return base.ReadSByte();
        }

        public override short ReadInt16()
        {
            return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            return base.ReadInt64();
        }

        public override byte ReadByte()
        {
            return base.ReadByte();
        }

        public override ushort ReadUInt16()
        {
            return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            return base.ReadDouble();
        }

        public override string ReadString()
        {
            char c = '\0';
            String str = "";
            while ((c = ReadChar()) != 0) str += c;
            return str;
        }

        public string ReadWString()
        {
            char c = '\0';
            String str = "";
            while ((c = ReadWChar()) != 0) str += c;
            return str;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void FillBuffer(int numBytes)
        {
            base.FillBuffer(numBytes);
        }
    }
}
