using System;
using Newtonsoft.Json;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class AwesomeCodec : IEncoder, IDecoder
    {
        private readonly OpinionatedSerializer _serializer;

        public AwesomeCodec()
        {
            _serializer = new OpinionatedSerializer();
        }

        public byte[] Encode(IotDevice device) => _serializer.Serialize(device);

        public IotDevice Decode(byte[] data) => _serializer.Deserialize(data);
    }
}