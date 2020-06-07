using System;
using Newtonsoft.Json;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs.JanReinhardt 
{
    // Demo implementation of a codec which uses json serialisation/deserialisation
    public class JsonCodec : IEncoder, IDecoder
    {
        public byte[] Encode(IotDevice device)
        {
            var text = JsonConvert.SerializeObject(device);

            var encoding = new System.Text.UTF8Encoding();

            return encoding.GetBytes(text);
        }

        public IotDevice Decode(byte[] data)
        {
            var encoding = new System.Text.UTF8Encoding();

            var text = encoding.GetString(data);

            return JsonConvert.DeserializeObject<IotDevice>(text);
        }
    }
}