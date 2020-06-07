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
            var codec = new Codecs.JanReinhardt.JsonCodec();

            var seq = new TransmissionSequence();

            // 30days * 24h * 60min * 60s = 2592000
            long messageCount = 30*24*60*60;

            var totalBytesTransmitted = seq.Run(source, codec, codec, messageCount, false);

            Console.WriteLine($"Total bytes transmitted: {totalBytesTransmitted}");
            Console.WriteLine(FormattableString.Invariant($"({totalBytesTransmitted/1024.0:F1}kB, {totalBytesTransmitted/(1024*1024):F1}MB)"));
        }
    }
}
