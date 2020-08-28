// From https://stackoverflow.com/questions/7051939/bit-based-binarywriter-in-c-sharp
using System;
using System.IO;
using System.Collections;

namespace ProgrammingChallenge2.Codecs.Matthid
{
    internal class BinaryWriter : System.IO.BinaryWriter
    {
        private bool[] curByte = new bool[8];
        private byte curBitIndx = 0;
        private System.Collections.BitArray ba;

        public BinaryWriter(Stream s) : base(s) { }

        public override void Flush()
        {
            base.Write(ConvertToByte(curByte));
            base.Flush();
        }

        public override void Write(bool value)
        {
            curByte[curBitIndx] = value;
            curBitIndx++;

            if (curBitIndx == 8)
            {
                base.Write(ConvertToByte(curByte));
                this.curBitIndx = 0;
                this.curByte = new bool[8];
            }
        }

        public override void Write(byte value)
        {
            ba = new BitArray(new byte[] { value });
            for (byte i = 0; i < 8; i++)
            {
                this.Write(ba[i]);
            }
            ba = null;
        }

        public override void Write(float value)
        {
            var buffer = BitConverter.GetBytes(value);
            Write(buffer);
        }

        public override void Write(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                this.Write((byte)buffer[i]);
            }
        }

        public override void Write(uint value)
        {
            ba = new BitArray(BitConverter.GetBytes(value));
            for (byte i = 0; i < 32; i++)
            {
                this.Write(ba[i]);
            }
            ba = null;
        }

        public override void Write(ulong value)
        {
            ba = new BitArray(BitConverter.GetBytes(value));
            for (byte i = 0; i < 64; i++)
            {
                this.Write(ba[i]);
            }
            ba = null;
        }

        public override void Write(ushort value)
        {
            ba = new BitArray(BitConverter.GetBytes(value));
            for (byte i = 0; i < 16; i++)
            {
                this.Write(ba[i]);
            }
            ba = null;
        }

        private static byte ConvertToByte(bool[] bools)
        {
            byte b = 0;

            byte bitIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (bools[i])
                {
                    b |= (byte)(((byte)1) << bitIndex);
                }
                bitIndex++;
            }

            return b;
        }
    }
    internal class BinaryReader : System.IO.BinaryReader
    {
        private bool[] curByte = new bool[8];
        private byte curBitIndx = 0;
        private BitArray ba;

        public BinaryReader(Stream s) : base(s)
        {
            ba = new BitArray(new byte[] { base.ReadByte() });
            ba.CopyTo(curByte, 0);
            ba = null;
        }

        public override bool ReadBoolean()
        {
            if (curBitIndx == 8)
            {
                ba = new BitArray(new byte[] { base.ReadByte() });
                ba.CopyTo(curByte, 0);
                ba = null;
                this.curBitIndx = 0;
            }

            bool b = curByte[curBitIndx];
            curBitIndx++;
            return b;
        }

        public override byte ReadByte()
        {
            bool[] bar = new bool[8];
            byte i;
            for (i = 0; i < 8; i++)
            {
                bar[i] = this.ReadBoolean();
            }

            byte b = 0;
            byte bitIndex = 0;
            for (i = 0; i < 8; i++)
            {
                if (bar[i])
                {
                    b |= (byte)(((byte)1) << bitIndex);
                }
                bitIndex++;
            }
            return b;
        }

        public override byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = this.ReadByte();
            }
            return bytes;
        }

        public override ushort ReadUInt16()
        {
            byte[] bytes = ReadBytes(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] bytes = ReadBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public override ulong ReadUInt64()
        {
            byte[] bytes = ReadBytes(8);
            return BitConverter.ToUInt64(bytes, 0);
        }
        
        public override float ReadSingle() {
            var buffer = ReadBytes(4);
            return BitConverter.ToSingle(buffer);
        }

    }
}