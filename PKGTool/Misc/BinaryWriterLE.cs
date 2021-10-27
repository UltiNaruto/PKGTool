using System.Linq;
using System.Text;

namespace System.IO
{
    public class BinaryWriterLE : BinaryWriter
    {
        public BinaryWriterLE(Stream input) : base(input)
        {
        }

        public BinaryWriterLE(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public BinaryWriterLE(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
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

        public override void Write(Char c)
        {
            Write(c, false);
        }

        public void Write(Char c, bool isUnicode)
        {
            if(isUnicode)
                Write((UInt16)c);
            else
                Write((Byte)c);
        }

        public override void Write(Char[] ch)
        {
            Write(ch, false);
        }

        public void Write(Char[] ch, bool isUnicode)
        {
            for (int i = 0; i < ch.Length; i++)
            {
                if (isUnicode)
                    Write(i < ch.Length ? ch[i] : (char)0, true);
                else
                    Write(i < ch.Length ? ch[i] : (char)0);
            }
        }

        public override void Write(decimal d)
        {
            base.Write(d);
        }

        public override void Write(SByte v)
        {
            base.Write(v);
        }

        public override void Write(Int16 v)
        {
            base.Write(v);
        }

        public override void Write(Int32 v)
        {
            base.Write(v);
        }

        public override void Write(Int64 v)
        {
            base.Write(v);
        }

        public override void Write(UInt16 v)
        {
            Write(v);
        }

        public override void Write(UInt32 v)
        {
            Write(v);
        }

        public override void Write(UInt64 v)
        {
            Write(v);
        }

        public override void Write(Single v)
        {
            base.Write(v);
        }

        public override void Write(Double v)
        {
            base.Write(v);
        }

        public override void Write(String str)
        {
            Write(str, false);
        }

        public void Write(String str, bool isUnicode)
        {
            foreach (var c in str)
            {
                if (isUnicode)
                    Write(c, true);
                else
                    Write(c);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
