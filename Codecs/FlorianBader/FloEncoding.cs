using System.Text;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public static class FloEncoding
    {
        private const int StartCharacterIndex = 'a';

        public static string GetString(byte[] bytes, bool onlyCharacters = false)
        {
            var bitReader = new BitReader(bytes);
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                var b = bitReader.ReadByte() - 1;

                if (onlyCharacters)
                {
                    var c = (char)(StartCharacterIndex + b);
                    stringBuilder.Append(c);
                }
                else
                {
                    if (b <= 9)
                    {
                        stringBuilder.Append(b);
                    }
                    else
                    {
                        var c = (char)(StartCharacterIndex + b - 10);
                        stringBuilder.Append(c);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public static byte[] GetBytes(string str, bool onlyCharacters = false)
        {
            var bitWriter = new BitWriter();
            for (int i = 0; i < str.Length; i++)
            {
                var c = (byte)str[i];

                if (onlyCharacters)
                {
                    var b = (byte)(c - StartCharacterIndex + 1);
                    bitWriter.WriteByte(b, offset: 3, bitCount: 5);
                }
                else
                {
                    if ((c - 48) <= 9)
                    {
                        bitWriter.WriteByte((byte)(c - 48 + 1), offset: 2, bitCount: 6);
                    }
                    else
                    {
                        var b = (byte)(c - StartCharacterIndex + 11);
                        bitWriter.WriteByte(b, offset: 2, bitCount: 6);
                    }
                }
            }

            return bitWriter.ToArray();
        }
    }
}