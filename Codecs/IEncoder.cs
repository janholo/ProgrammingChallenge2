using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs
{
    public interface IEncoder
    {
        byte[] Encode(IotDevice device);
    }
}