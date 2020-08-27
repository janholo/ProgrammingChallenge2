namespace ProgrammingChallenge2.Codecs.FlorianBader
{
    public class AwesomeCodecFactory : ICodecFactory
    {
        public string Name => "AwesomeCodec by Florian Bader";

        public IDecoder CreateDecoder() => new AwesomeCodec();

        public IEncoder CreateEncoder() => new AwesomeCodec();
    }
}