using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    public class TotallyLegitCodec : IEncoder, IDecoder
    {
        private Random _random = new Random(42);

        public byte[] Encode(IotDevice device)
        {
            return new byte[_random.Next(23)];
        }

        public IotDevice Decode(byte[] data)
        {
            return new IotDevice("YOLO", null, null, false, false, 0UL, new PhysicalValue(0, null), new PhysicalValue(0, null), new PhysicalValue(0, null));
        }
    }
}