using Misc.Structs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dread.FileFormats
{
    public class PKG : BinaryStruct
    {
        public List<KeyValuePair<UInt64, MemoryStream>> Files = new List<KeyValuePair<UInt64, MemoryStream>>();
        public bool IgnoreDataSectionSizeCheck = false;

        public void Close()
        {
            foreach (var file in Files)
                file.Value.Close();
            Files.Clear();
        }

        Int32[,] GenerateFileOffsets()
        {
            Int32 i = 0;
            Int32[,] offsets = new Int32[Files.Count, 2];
            Int32 cursor = 12 + Files.Count * 16;
            cursor = cursor.Aligned(128);
            foreach (var file in Files)
            {
                offsets[i, 0] = cursor;
                cursor += (Int32)file.Value.Length;
                offsets[i, 1] = cursor;
                cursor = cursor.Aligned(8);
                i++;
            }
            return offsets;
        }

        public override int StructSize
        {
            get
            {
                int len = 12 + Files.Count * 16;
                len = len.Aligned(128);
                foreach (var file in Files)
                {
                    len += (int)file.Value.Length;
                    len = len.Aligned(128);
                }
                return len;
            }
        }

        public override void import(Stream stream)
        {
            Int32 i, data_section_start;
            List<UInt64> IDs = new List<UInt64>();
            List<Int32[]> Offsets = new List<Int32[]>();

            var reader = new BinaryReaderLE(stream);
            Int32 header_size = reader.ReadInt32();
            Int32 data_section_size = reader.ReadInt32();
            Int32 file_count = reader.ReadInt32();

            for(i = 0; i< file_count;i++)
            {
                Files.Add(new KeyValuePair<UInt64, MemoryStream>(reader.ReadUInt64(), new MemoryStream()));
                Offsets.Add(new Int32[] { reader.ReadInt32(), reader.ReadInt32() });
            }

            // padding
            stream.Position = stream.Position.Aligned(128) - 4;

            if (header_size != (Int32)stream.Position)
                throw new Exception("Invalid PKG file! (Guessed header size doesn't correspond to the real size)");

            stream.Position = stream.Position.Aligned(128);

            data_section_start = (Int32)stream.Position;

            for (i=0;i<file_count;i++)
            {
                if (Offsets[i][0] != (int)stream.Position)
                    throw new Exception("Wrong starting offset!");
                Files[i].Value.Write(reader.ReadBytes(Offsets[i][1] - Offsets[i][0]));
                Files[i].Value.Position = 0L;

                // padding
                stream.Position = stream.Position.Aligned(8);
            }

            if (!IgnoreDataSectionSizeCheck && data_section_size != (Int32)stream.Position - data_section_start)
                throw new Exception("Invalid PKG file! (Guessed data section size doesn't correspond to the real size)");
        }

        public override void export(Stream stream)
        {
            Int32 i, header_size, data_section_start, data_section_size;
            Int32[,] offsets = GenerateFileOffsets();

            var writer = new BinaryWriterLE(stream);
            stream.Position += 8;
            writer.Write(Files.Count);
            for(i=0;i<Files.Count;i++)
            {
                writer.Write(Files[i].Key);
                writer.Write(offsets[i, 0]);
                writer.Write(offsets[i, 1]);
            }

            // padding
            while ((stream.Position + 4) % 128 != 0) writer.Write((byte)0);

            header_size = (Int32)stream.Position;

            // padding
            while (stream.Position % 128 != 0) writer.Write((byte)0);

            data_section_start = (Int32)stream.Position;

            for (i = 0; i < Files.Count; i++)
            {
                if (offsets[i, 0] != (int)stream.Position)
                    throw new Exception("Wrong starting offset!");
                Files[i].Value.CopyTo(stream);
                Files[i].Value.Position = 0L;

                // padding
                while (stream.Position % 8 != 0) writer.Write((byte)0);
            }

            data_section_size = (Int32)stream.Position - data_section_start;
            
            stream.Position = 0L;

            writer.Write(header_size);
            writer.Write(data_section_size);
        }
    }
}
