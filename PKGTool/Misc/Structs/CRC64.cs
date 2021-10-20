using System;
using System.Diagnostics;
using System.Text;

namespace HashLib.Checksum
{
    public class CRC64
    {
        private ulong[] m_crc_tab = new ulong[256];
        private ulong m_initial_value;
        private ulong m_final_xor;

        public CRC64()
        {
            m_initial_value = ulong.MaxValue;
            m_final_xor = 0UL;

            GenerateCRCTable(0x42F0E1EBA9EA3693);
        }

        private byte Reflect8(byte val)
        {
            byte res = 0;
            for(int i=0;i<8;i++)
            {
                if ((val & (1 << i)) != 0)
                    res |= (byte)((1 << (7 - i)) & 0xFF);
            }
            return res;
        }

        private ulong Reflect64(ulong val)
        {
            ulong res = 0;
            for (int i = 0; i < 64; i++)
            {
                if ((val & (1UL << i)) != 0UL)
                    res |= (1UL << (63 - i)) & UInt64.MaxValue;
            }
            return res;
        }

        private void GenerateCRCTable(ulong a_poly64)
        {
            ulong castMask = ulong.MaxValue;
            ulong msbMask = (ulong)long.MaxValue + 1UL;
            ulong curByte = 0UL;
            for (ulong divident = 0; divident < 256; divident++)
            {
                curByte = (divident << 56) & castMask;

                for (uint j = 0; j < 8; j++)
                {
                    if ((curByte & msbMask) != 0)
                        curByte = (curByte << 1) ^ a_poly64;
                    else
                        curByte <<= 1;
                }

                m_crc_tab[divident] = curByte & castMask;
            }
        }

        public byte[] Compute(byte[] a_data, int a_index, int a_length)
        {
            Debug.Assert(a_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_index + a_length <= a_data.Length);

            ulong castMask = ulong.MaxValue;
            ulong m_hash = m_initial_value;
            ulong curByte = 0UL;
            int pos = 0;

            for (int i = a_index; a_length > 0; i++, a_length--)
            {
                curByte = (ulong)Reflect8(a_data[i]) << 56;
                m_hash ^= curByte;
                m_hash &= castMask;

                pos = (int)((m_hash >> 56) & 0xFF);
                m_hash <<= 8;
                m_hash &= castMask;
                m_hash ^= m_crc_tab[pos];
                m_hash &= castMask;
            }

            m_hash = Reflect64(m_hash);
            m_hash = (m_hash ^ m_final_xor) & castMask;

            return BitConverter.GetBytes(m_hash);
        }

        public ulong ComputeAsValue(byte[] a_data, int a_index, int a_length)
        {
            return BitConverter.ToUInt64(Compute(a_data, a_index, a_length), 0);
        }

        public String ComputeAsString(byte[] a_data, int a_index, int a_length)
        {
            return String.Format("{0:X8}", ComputeAsValue(a_data, a_index, a_length));
        }

        public byte[] Compute(String text, String encoding = "UTF-8")
        {
            return Compute(Encoding.GetEncoding(encoding).GetBytes(text), 0, text.Length);
        }

        public ulong ComputeAsValue(String text, String encoding = "UTF-8")
        {
            return BitConverter.ToUInt64(Compute(Encoding.GetEncoding(encoding).GetBytes(text), 0, text.Length), 0);
        }

        public String ComputeAsString(String text, String encoding = "UTF-8")
        {
            return String.Format("{0:X8}", ComputeAsValue(Encoding.GetEncoding(encoding).GetBytes(text), 0, text.Length));
        }
    }
}