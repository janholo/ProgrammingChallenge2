using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs
{
    public interface IDecoder
    {
        IotDevice Decode(byte[] data);
    }
}