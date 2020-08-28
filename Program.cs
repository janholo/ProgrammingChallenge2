using System;
using ProgrammingChallenge2.Codecs;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var source = new DataSource();

            var codecFactory = new Codecs.JanReinhardt.JsonCodecFactory();

            var seq = new TransmissionSequence();

            // 1days * 24h * 60min * 60s = 2592000
            long messageCount = 1 * 24 * 60 * 60;

            Console.WriteLine("Start...");

            var totalBytesTransmitted = seq.Run(source, codecFactory.CreateEncoder(), codecFactory.CreateDecoder(), messageCount, false);

            Console.WriteLine($"Result for Codec '{codecFactory.Name}'");
            Console.WriteLine($"Total bytes transmitted: {totalBytesTransmitted}");
            Console.WriteLine(FormattableString.Invariant($"({totalBytesTransmitted / 1024.0:F1}kB, {totalBytesTransmitted / (1024 * 1024):F1}MB)"));
        }
    }
}
