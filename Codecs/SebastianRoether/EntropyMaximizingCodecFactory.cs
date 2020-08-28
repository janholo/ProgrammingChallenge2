namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    public class EntropyMaximizingCodecFactory : ICodecFactory
    {
        public string Name => "Codec that maximizes entropy per message by Sebastian Roether";

        public IDecoder CreateDecoder() => new EntropyMaximizingCodec(false);

        public IEncoder CreateEncoder() => new EntropyMaximizingCodec(false);
    }
}