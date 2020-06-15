namespace ProgrammingChallenge2.Codecs.JanReinhardt 
{
    // Demo implementation of the factory
    public class JsonCodecFactory : ICodecFactory
    {
        public string Name => "JsonCodec by Jan Reinhardt";

        public IDecoder CreateDecoder() => new JsonCodec();

        public IEncoder CreateEncoder() => new JsonCodec();
    }
}