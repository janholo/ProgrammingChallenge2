using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.JanReinhardt 
{
    public class JsonCodec : IEncoder, IDecoder
    {
        public byte[] Encode(IotDevice device)
        {
            throw new NotImplementedException();
        }

        public IotDevice Decode(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}