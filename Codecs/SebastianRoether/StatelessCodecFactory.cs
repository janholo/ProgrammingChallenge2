namespace ProgrammingChallenge2.Codecs.SebastianRoether 
{
    public class StatelessCodecFactory : ICodecFactory
    {
        public string Name => "Stateless unopinionated codec by Sebastian Roether";

        public IDecoder CreateDecoder() => new StatelessCodec();

        public IEncoder CreateEncoder() => new StatelessCodec();
    }
}