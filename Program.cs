using System;
using ProgrammingChallenge2.Codecs;

namespace ProgrammingChallenge2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 1days * 24h * 60min * 60s = 2592000
            long messageCount = 1*24*60*60;

            Run(messageCount, new Codecs.JanReinhardt.JsonCodecFactory());
        }

        private static long Run(long messageCount, ICodecFactory codecFactory)
        {
            var source = new DataSource();

            var seq = new TransmissionSequence();

            Console.WriteLine();
            Console.WriteLine("Start...");

            var totalBytesTransmitted = seq.Run(source, codecFactory.CreateEncoder(), codecFactory.CreateDecoder(), messageCount, false);

            Console.WriteLine($"Result for Codec '{codecFactory.Name}'");
            Console.WriteLine($"Total bytes transmitted: {totalBytesTransmitted}");
            Console.WriteLine(FormattableString.Invariant($"({totalBytesTransmitted/1024.0:F1}kB, {totalBytesTransmitted/(1024*1024):F1}MB)"));

            return totalBytesTransmitted;
        }
    }
}
