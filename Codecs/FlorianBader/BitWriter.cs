using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class BitWriter
    {
        private readonly List<bool> _bits;

        public BitWriter()
        {
            _bits = new List<bool>();
        }

        public void WriteByte(byte b, int offset = 0, int bitCount = 8)
        {
            for (int i = offset; i < bitCount + offset; i++)
            {
                var bit = BitOperations.GetBit(b, i);
                _bits.Add(bit);
            }
        }

        public void WriteBytes(byte[] bytes)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                WriteByte(bytes[i]);
            }
        }

        public void WriteBit(byte bit)
        {
            _bits.Add((bit & 1) == 1);
        }

        public void WriteBit(bool bit)
        {
            _bits.Add(bit);
        }

        public void WriteBits(byte[] bytes, int length, int offset = 0)
        {
            for (int i = offset; i < length + offset; i++)
            {
                var byteIndex = i / 8;
                var bitIndex = i % 8;
                var bit = BitOperations.GetBit(bytes[byteIndex], bitIndex);
                _bits.Add(bit);
            }
        }

        public byte[] ToArray()
        {
            var bytes = new byte[_bits.Count / 8 + 1];
            for (int i = 0; i < _bits.Count; i++)
            {
                var byteIndex = i / 8;
                var bitIndex = i % 8;

                if (_bits[i])
                {
                    BitOperations.SetBit(ref bytes[byteIndex], bitIndex);
                }
            }

            return bytes;
        }
    }
}