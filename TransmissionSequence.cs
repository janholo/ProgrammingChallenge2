using System;
using ProgrammingChallenge2.Codecs;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    public class TransmissionSequence
    {
        public long Run(IDataSource source, IEncoder encoder, IDecoder decoder, long msgCount, bool debug)
        {
            long totalBytesTransmitted = 0;

            for (long i = 0; i < msgCount; i++)
            {
                var sourceData = source.GetNextDataPoint();

                var encodedData = encoder.Encode(sourceData);

                // This is where the data is normally transmitted via network
                totalBytesTransmitted += encodedData.LongLength;

                var decodedData = decoder.Decode(encodedData);

                var areEqual = IotDevice.AreEquals(sourceData, decodedData, false);

                if (!areEqual || debug)
                {
                    Console.WriteLine();
                    Console.WriteLine("##########################################################");
                    Console.WriteLine($"Message number: {i+1}");
                    Console.WriteLine();

                    Console.WriteLine("source data:");
                    Console.WriteLine(sourceData);
                    Console.WriteLine();

                    Console.WriteLine("encoded data:");
                    Console.WriteLine(BitConverter.ToString(encodedData).Replace("-", ""));
                    Console.WriteLine();

                    Console.WriteLine("decoded data:");
                    Console.WriteLine(decodedData);
                    Console.WriteLine();

                    if(!areEqual)
                    {
                        // run this again to get debug outputs
                        IotDevice.AreEquals(sourceData, decodedData, true);
                    }

                    Console.WriteLine($"Encoding/Decoding successful: {areEqual}");
                }

                if (debug)
                {
                    Console.WriteLine("Press any key to send the next message.");
                    Console.ReadKey();
                }
            }

            return totalBytesTransmitted;
        }
    }
}