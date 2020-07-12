using System.Collections;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class BitReader
    {
        private byte[] _bytes;
        private int _bitIndex;

        public int BitIndex => _bitIndex;

        public BitReader(byte[] bytes)
        {
            _bytes = bytes;
            _bitIndex = 0;
        }

        public byte[] ReadBytes(int length, int bitLength = 8)
        {
            var bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                bytes[i] = ReadByte(bitLength);
            }

            return bytes;
        }

        public byte ReadByte(int bitLength = 8)
        {
            var b = GetByte(_bitIndex, bitLength);
            _bitIndex += bitLength;
            return b;
        }

        public bool ReadBit()
        {
            var bit = GetCurrentBit();
            _bitIndex++;

            return bit;
        }

        public int IndexOf(in byte searchByte)
        {
            for (var i = _bitIndex; i < _bytes.Length * 8; i += 8)
            {
                var peekedByte = GetByte(i);
                if (peekedByte == searchByte)
                {
                    return (i - _bitIndex) / 8;
                }
            }

            return -1;
        }

        public int IndexOfBit(in byte searchByte, int bitLength = 8)
        {
            for (var i = _bitIndex; i < _bytes.Length * 8; i++)
            {
                var peekedByte = GetByte(i, bitLength);
                if (peekedByte == searchByte)
                {
                    return _bitIndex;
                }
            }

            return -1;
        }

        private byte GetByte(int index, int bitLength = 8)
        {
            var b = (byte)0;
            for (int i = 0; i < bitLength; i++)
            {
                var value = GetBit(_bytes, index + i);
                if (value)
                {
                    BitOperations.SetBit(ref b, i);
                }
            }

            // shift to right so the value goes from lowest to highest bit
            b = (byte)(b >> (8 - bitLength));

            return b;
        }

        private bool GetCurrentBit()
        {
            var value = GetBit(_bytes, _bitIndex);
            return value;
        }

        private bool GetBit(in byte[] bytes, int bitIndex)
        {
            var readingByteIndex = bitIndex / 8;
            var readingBitIndex = bitIndex % 8;

            var value = BitOperations.GetBit(bytes[readingByteIndex], readingBitIndex);
            return value;
        }
    }
}