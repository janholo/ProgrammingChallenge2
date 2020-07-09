using System;
using Newtonsoft.Json;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class AwesomeCodec : IEncoder, IDecoder
    {
        private readonly DiffSerializer _serializer;

        public AwesomeCodec()
        {
            _serializer = new DiffSerializer();
        }

        public byte[] Encode(IotDevice device) => _serializer.Serialize(device);

        public IotDevice Decode(byte[] data) => _serializer.Deserialize(data);
    }
}