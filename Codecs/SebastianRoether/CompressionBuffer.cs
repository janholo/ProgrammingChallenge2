namespace ProgrammingChallenge2.Codecs.SebastianRoether
{
    using System;
    using System.Linq;

    public class CompressionBuffer
    {
        private int _p;
        private int _offset;
        private int?[] Buffer = new int?[1024];
        private bool[] Mask = new bool[1024];
        private bool debug;

        public CompressionBuffer(bool debug)
        {
            _p = 0;
            _offset = 21;
            this.debug = debug;
        }

        internal CompressionBuffer(int p, int offset, int?[] buffer, bool[] maskArray, bool debug)
        {
            _p = p;
            _offset = offset;
            Buffer = buffer;
            Mask = maskArray;
            this.debug = debug;
        }

        public CompressionBuffer Clone()
        {
            var copyArray = new int?[Buffer.Length];
            var maskClone = new bool[Mask.Length];
            Array.Copy(Buffer, copyArray, Buffer.Length);
            Array.Copy(Mask, maskClone, Mask.Length);
            return new CompressionBuffer(_p, _offset, copyArray, maskClone, debug);
        }

        private int? InternalDecompressAndUnfold()
        {
            int? retVal;
            int p = _p;
            int offset = _offset;

            if (++p >= 56) p = 1;
            if (++offset >= 56) offset = 1;

            retVal = Buffer[p] - Buffer[offset];

            if (retVal == Int32.MaxValue) retVal--;
            if (retVal < 0) retVal += Int32.MaxValue;

            Buffer[p] = retVal;

            Mask[p] &= Mask[offset];

            _p = p;
            _offset = offset;

            return retVal;
        }

        public void CompressAndFold(double value, int check)
        {
            InternalCompressAndFold((int)((double)(value / check) * Int32.MaxValue), check);
        }

        public void CompressAndFold(int bytes)
        {
            for(int i = 0; i < bytes; i++)
            {
                InternalCompressAndFold(null, 0);
            }
        }

        private void InternalCompressAndFold(int? value, int check)
        {
            int retVal;
            int lp = _p;
            int offset = _offset;

            if (++lp >= 56) lp = 1;
            if (++offset >= 56) offset = 1;

            if (Buffer[lp] == null || Buffer[offset] == null)
            {
                Buffer[lp] = value;
                Mask[lp] = value != null && Magic(value.Value, check);
                _p = lp;
                _offset = offset;
            }
            else
            {
                retVal = Buffer[lp].Value - Buffer[offset].Value;

                if (retVal == Int32.MaxValue) retVal--;
                if (retVal < 0) retVal += Int32.MaxValue;

                if ((retVal != value) && value != null)
                {
                    if(!Mask[lp] || !Mask[offset])
                    {
                        Buffer[lp] = value;
                        Mask[lp] = Magic(value.Value, check);
                    }
                    else
                    {
                        Mask[lp] = true;
                        Buffer[lp] = retVal;
                    }
                }
                else
                {
                    Buffer[lp] = retVal;
                    Mask[lp] = (Mask[lp] && Mask[offset]);
                }
                _p = lp;
                _offset = offset;
            }
        }

        public virtual int? DecompressSimple()
        {
            return (int?)(InternalDecompressAndUnfold() * (1.0 / Int32.MaxValue) * 25);
        }

        public virtual double? Decompress()
        {
            return InternalDecompressAndUnfold() * (1.0 / Int32.MaxValue);;
        }

        private bool Magic(int i, int check)
        {
            return Voodoo(i - 1, check) & Voodoo(i, check) & Voodoo(i + 1, check);
        }

        private bool Voodoo(int i, int check)
        {
            double x = i * (1.0 / Int32.MaxValue);
            double y = x * check;
            double z = y / check;
            int v = (int)(z * Int32.MaxValue);
            return (v == i);
        }
    }
}