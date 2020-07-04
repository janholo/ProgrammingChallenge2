using System.Collections;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class BitReader
    {
        private byte[] _bytes;
        private int _bitIndex;

        public BitReader(byte[] bytes)
        {
            _bytes = bytes;
            _bitIndex = 0;
        }

        public byte[] ReadBytes(int length)
        {
            var bytes = new byte[length];
            bytes.Initialize();

            for (int i = 0; i < length * 8; i++)
            {
                var value = GetCurrentBit();
                if (value)
                {
                    var byteIndex = i / 8;
                    var bitIndex = i % 8;
                    BitOperations.SetBit(ref bytes[byteIndex], bitIndex);
                }

                _bitIndex++;
            }

            return bytes;
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

        private byte GetByte(int index)
        {
            var b = (byte)0;
            for (int i = 0; i < 8; i++)
            {
                var value = GetBit(_bytes, index + i);
                if (value)
                {
                    BitOperations.SetBit(ref b, i);
                }
            }

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