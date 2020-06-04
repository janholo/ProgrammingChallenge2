using System;
using ProgrammingChallenge2.Model;

namespace ProgrammingChallenge2.Codecs
{
    interface IEncoder
    {
        byte[] Encode(IotDevice device);
    }
}