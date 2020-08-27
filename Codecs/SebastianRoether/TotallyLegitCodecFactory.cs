namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    // Demo implementation of the factory
    public class TotallyLegitCodecFactory : ICodecFactory
    {
        public string Name => "TotallyLegitCodec by Sebastian Roether";

        public IDecoder CreateDecoder() => new TotallyLegitCodec();

        public IEncoder CreateEncoder() => new TotallyLegitCodec();
    }
}