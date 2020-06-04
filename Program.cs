using System;
using ProgrammingChallenge2.Codecs;
using ProgrammingChallenge2.Codecs.JanReinhardt;

namespace ProgrammingChallenge2
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new RandomDataSource();
            IEncoder encoder = new JsonCodec();
            IDecoder decoder = new JsonCodec();

            var data = source.GetNextDataPoint();

            Console.WriteLine("original data:");
            Console.WriteLine(data);

            var encodedData = encoder.Encode(data);
            Console.WriteLine("encoded data:");
            Console.WriteLine(BitConverter.ToString(encodedData).Replace("-", ""));
            

            var decodedData = decoder.Decode(encodedData);
            Console.WriteLine("decoded data:");
            Console.WriteLine(decodedData);
        }
    }
}
