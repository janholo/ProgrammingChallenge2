using System;
using Newtonsoft.Json;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class AwesomeCodec : IEncoder, IDecoder
    {
        private readonly AwesomeBinarySerializer _serializer;

        public AwesomeCodec()
        {
            _serializer = new AwesomeBinarySerializer();
        }

        public byte[] Encode(IotDevice device) => _serializer.Serialize(device);

        public IotDevice Decode(byte[] data) => _serializer.Deserialize<IotDevice>(data);
    }
}