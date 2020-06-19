using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProgrammingChallenge2.Codecs;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const bool debug = false;

            // 1days * 24h * 60min * 60s = 2592000
            const long messageCount = 1 * 24 * 60 * 60;

            var source = new DataSource();

            var seq = new TransmissionSequence();

            Console.WriteLine(debug ? "Start in debug mode...." : "Start...");
            Console.WriteLine($"Message count: {messageCount:N0}");

            var codecTypes = GetCodecFactories();

            foreach (var codecType in codecTypes)
            {
                var codecFactory = Activator.CreateInstance(codecType) as ICodecFactory;
                var encoder = codecFactory.CreateEncoder();
                var decoder = codecFactory.CreateDecoder();

                Console.WriteLine($"[{codecFactory.Name}] Starting codec {codecFactory.Name}");

                Console.WriteLine($"[{codecFactory.Name}] Running warmup...");
                seq.Run(source, encoder, decoder, 10, false);

                Console.WriteLine($"[{codecFactory.Name}] Start measuring...");

                var start = Stopwatch.GetTimestamp();
                var totalBytesTransmitted = seq.Run(source, encoder, decoder, messageCount, debug);

                var elapsed = TimeSpan.FromMilliseconds((Stopwatch.GetTimestamp() - start) / (Stopwatch.Frequency / 1000));

                Console.WriteLine($"[{codecFactory.Name}] Result for codec {codecFactory.Name}");
                Console.WriteLine($"[{codecFactory.Name}] Total bytes transmitted: {totalBytesTransmitted:N0}");
                Console.WriteLine($"[{codecFactory.Name}] Duration: {elapsed:mm\\:ss\\.FFFFFFF}");
                Console.WriteLine(FormattableString.Invariant($"[{codecFactory.Name}] ({totalBytesTransmitted / 1024:N1}KB, {totalBytesTransmitted / (1024 * 1024):N1}MB)"));
            }
        }

        private static IEnumerable<Type> GetCodecFactories()
        {
            var codecTypes = typeof(ICodecFactory).Assembly
                .GetTypes()
                .Where(t => typeof(ICodecFactory).IsAssignableFrom(t) && !t.IsAbstract);

            var hasRunOnly = codecTypes.FirstOrDefault(t => t.GetCustomAttributes(inherit: false).Any(c => c is RunOnlyThisAttribute));
            if (hasRunOnly is object)
            {
                codecTypes = new[] { hasRunOnly };

                var codecFactory = Activator.CreateInstance(hasRunOnly) as ICodecFactory;
                Console.WriteLine($"Run only {codecFactory.Name}");
            }

            return codecTypes;
        }
    }
}
