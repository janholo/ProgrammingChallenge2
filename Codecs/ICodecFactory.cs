namespace ProgrammingChallenge2.Codecs
{
    public interface ICodecFactory
    {
        string Name { get; }

        IEncoder CreateEncoder();

        IDecoder CreateDecoder();
    }
}