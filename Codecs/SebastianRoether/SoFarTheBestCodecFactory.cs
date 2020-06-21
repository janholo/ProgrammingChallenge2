namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    // Demo implementation of the factory
    public class SoFarTheBestCodecFactory : ICodecFactory
    {
        public string Name => "SoFarTheBestCodec by Sebastian Roether";

        public IDecoder CreateDecoder() => new SoFarTheBestCodec();

        public IEncoder CreateEncoder() => new SoFarTheBestCodec();
    }
}