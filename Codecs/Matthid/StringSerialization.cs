using System.Text;
using System.IO;
using System;

namespace ProgrammingChallenge2.Codecs.Matthid
{
    // we need 5 bits to represent all 26 lowercase characters (indeed we can represent 31 characters with this, so we include the space)
    public class ImprovedLowerCaseStringEncoder {
        internal static void WriteString(BinaryWriter bitWriter, string data) {
            foreach(char c in data) {
                int charNum = c - 'a' + 1;
                if (c==' '){
                    // append space
                    charNum = 'z' - 'a' + 2;
                }
                if (charNum > 32 || charNum <= 0){
                    throw new InvalidOperationException($"this encoder cannot encode the character '{c}'");
                }

                // Write every char with big endian
                for (int i = 0; i < 5; i++)
                    bitWriter.Write((charNum & (1 << i)) != 0);
            }
        }
        public static byte[] EncodeString(string data) {
            var memory = new MemoryStream();
            var bitWriter = new BinaryWriter(memory);

            WriteString(bitWriter, data);
            bitWriter.Flush();
            return memory.ToArray();
        }

        
        internal static string ReadString(BinaryReader bitReader, int charCount = -1) {

            var builder = new StringBuilder();
            try {
                int toRead = charCount;
                while (toRead == -1 || toRead-- > 0){
                    
                    int charNum = 0;
                    // Read every char with big endian
                    for (int i = 0; i < 5; i++)
                    {
                        if (bitReader.ReadBoolean()){                        
                            charNum = charNum | (1 << i);
                        }
                    }

                    if (charNum > 0 && charNum <= 'z' - 'a' + 1){
                        char c = (char)(charNum + 'a' - 1);
                        builder.Append(c);
                    } else if (charNum == 'z' - 'a' + 2){
                        builder.Append(' ');
                    } else if (charNum == 0) {
                        // padding
                        break;
                    } else {
                        throw new InvalidOperationException($"This decoder doesn't expect characters outside of lowercase asci, but got '{charNum}'");
                    }
                }
            } catch (EndOfStreamException) {
                if (charCount != -1 && builder.Length != charCount){
                    throw;
                }

                // Good
            }

            return builder.ToString();
        }

        public static string DecodeBytes(byte[] data) {
            var memory = new MemoryStream(data);
            var bitReader = new BinaryReader(memory);
            return ReadString(bitReader);
        }
    }
}