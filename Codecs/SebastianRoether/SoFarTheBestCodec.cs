
using ProgrammingChallenge2.Model;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    // Lame attempt at winning the contest with minimal effort and a suboptimal codec
    public class SoFarTheBestCodec : IEncoder, IDecoder
    {
        public byte[] Encode(IotDevice device)
        {
            var text = JsonConvert.SerializeObject(device);
            var encoding = new System.Text.UTF8Encoding();
            using var memoryStream = new MemoryStream();

            // Holy shit, thats slow
            using var brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal);
            brotliStream.Write(encoding.GetBytes(text));
            brotliStream.Flush();
            return memoryStream.ToArray();
        }

        public IotDevice Decode(byte[] data)
        {
            var decodedBytes = new byte[1000];
            var decoded = BrotliDecoder.TryDecompress(data, decodedBytes, out int bytesWritten);
            var encoding = new System.Text.UTF8Encoding();
            var text = encoding.GetString(decodedBytes);
            return JsonConvert.DeserializeObject<IotDevice>(text);   
        }
    }
}