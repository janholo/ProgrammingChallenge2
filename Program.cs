using System;
using ProgrammingChallenge2.Codecs;
using ProgrammingChallenge2.Codecs.JanReinhardt;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new DataSource();
            IEncoder encoder = new JsonCodec();
            IDecoder decoder = new JsonCodec();

            var data = source.GetNextDataPoint();
            Console.WriteLine("original data:");
            Console.WriteLine(data);
            Console.WriteLine();

            var encodedData = encoder.Encode(data);
            Console.WriteLine("encoded data:");
            Console.WriteLine(BitConverter.ToString(encodedData).Replace("-", ""));
            Console.WriteLine();

            var decodedData = decoder.Decode(encodedData);
            Console.WriteLine("decoded data:");
            Console.WriteLine(decodedData);
            Console.WriteLine();

            var isEqual = IotDevice.AreEquals(data, decodedData, true);
            Console.WriteLine($"Encoding/Decoding successful: {isEqual}");
        }
    }
}
