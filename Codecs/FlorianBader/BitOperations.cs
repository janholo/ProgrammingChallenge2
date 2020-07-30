namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public static class BitOperations
    {
        public static bool GetBit(in byte @byte, int bitIndex)
        {
            var bit = (1 << (7 - bitIndex));
            var value = (@byte & bit) == bit;
            return value;
        }

        public static void SetBit(ref byte @byte, int bitIndex)
        {
            @byte |= (byte)(1 << (7 - bitIndex));
        }
    }
}