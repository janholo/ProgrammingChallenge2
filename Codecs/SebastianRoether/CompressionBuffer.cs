namespace ProgrammingChallenge2.Codecs.SebastianRoether
{
    using System;
    using System.Linq;

    public class CompressionBuffer
    {
        private const int MBIG = Int32.MaxValue;
        private int inext;
        private int inextp;
        public int?[] SeedArray = new int?[56];
        public bool[] Mask = new bool[56];
        private bool debug;

        private const int MSEED = 161803398;

        public CompressionBuffer(bool debug)
        {
            inext = 0;
            inextp = 21;
            this.debug = debug;
        }

        internal CompressionBuffer(int inext, int inextp, int?[] buffer, bool[] maskArray, bool debug)
        {
            this.inext = inext;
            this.inextp = inextp;
            this.SeedArray = buffer;
            this.Mask = maskArray;
            this.debug = debug;
        }

        public CompressionBuffer Clone()
        {
            var copyArray = new int?[SeedArray.Length];
            var maskClone = new bool[Mask.Length];
            Array.Copy(SeedArray, copyArray, SeedArray.Length);
            Array.Copy(Mask, maskClone, Mask.Length);
            return new CompressionBuffer(inext, inextp, copyArray, maskClone, debug);
        }

        protected virtual double? Sample()
        {
            return (InternalSample() * (1.0 / MBIG));
        }

        private int? InternalSample()
        {
            int? retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            retVal = SeedArray[locINext] - SeedArray[locINextp];

            if (retVal == MBIG) retVal--;
            if (retVal < 0) retVal += MBIG;

            if(debug) Console.WriteLine("PREDICT Index " + locINext + "=" + retVal);
            SeedArray[locINext] = retVal;

            Mask[locINext] &= Mask[locINextp];

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        public void observe(double value, int check)
        {
            internalObserve((int)((double)(value / check) * MBIG), check);
        }

        public void miss()
        {
            internalObserve(null, 0);
        }

        public int P()
        {
            return SeedArray.Count(x => x == null);
        }
                public int Q()
        {
            return Mask.Count(x => x == true);
        }

        public void Dump()
        {
            Console.WriteLine("--------BUFFER---------");

            for (int i = 0; i < SeedArray.Length; i++)
            {
                Console.Write(SeedArray[i] + ",");
            }
            Console.WriteLine();
            for (int i = 0; i < SeedArray.Length; i++)
            {
                Console.Write(Mask[i] + ",");
            }

            Console.WriteLine();
            Console.WriteLine(SeedArray.Count(x => x == null));
            Console.WriteLine(Mask.Count(x => x == true));
        }

        private void internalObserve(int? value, int check)
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            if (SeedArray[locINext] == null || SeedArray[locINextp] == null)
            {
                SeedArray[locINext] = value;
                
                if(debug) Console.WriteLine("SET Index " + locINext + "=" + value);
                Mask[locINext] = value != null && Magic(value.Value, check); // && Magic(value.Value, 100) && Magic(value.Value, 1000);
                inext = locINext;
                inextp = locINextp;
            }
            else
            {
                retVal = SeedArray[locINext].Value - SeedArray[locINextp].Value;

                if (retVal == MBIG) retVal--;
                if (retVal < 0) retVal += MBIG;

                if ((retVal != value) && value != null)
                {
                    if (debug)
                    {
                        if (Math.Abs((long)retVal - (long)value.Value) > 0)
                        {
                            Console.WriteLine("actual value " + retVal + " does not match observation " + value + "(certain=" + (Mask[locINext] && Mask[locINextp]) + "). Other Value:" + SeedArray[locINextp].Value);
                        }
                    }

                    // prediction different than observation
                    if(!Mask[locINext] || !Mask[locINextp])
                    {
                        // no certainty, just take value
                        SeedArray[locINext] = value;
                        if(debug) Console.WriteLine("SET Index " + locINext + "=" + value + " after prediction mismatch (" + Mask[locINext] + " -> "+ Magic(value.Value, check));
                        Mask[locINext] = Magic(value.Value, check); // && Magic(value.Value, 100) && Magic(value.Value, 1000);
                    }
                    else
                    {
                        // mismatch but we know better than observation
                        Mask[locINext] = true;
                        if(debug) Console.WriteLine("SET Index " + locINext + "=" + retVal + " after prediction mismatch and we know better (was " + SeedArray[locINext] +")");
                        SeedArray[locINext] = retVal;
                    }
                }
                else
                {
                    SeedArray[locINext] = retVal;
                    Mask[locINext] = (Mask[locINext] && Mask[locINextp]); /*|| (value != null && Magic(value.Value, 5))*/;
                }
                inext = locINext;
                inextp = locINextp;
            }
        }

        public virtual int? Next(int maxValue)
        {
            return (int?)(Sample() * maxValue);
        }

        public virtual double? NextDouble()
        {
            return Sample();
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

        public int Align()
        {
            for(int i = 0; i < SeedArray.Length; i++)
            {
                for(int j = 0; j < SeedArray.Length; j++)
                {
                    if(ProgrammingChallenge2.Random._seedArray[j] == SeedArray[i])
                    {
                        return i - j;
                    }
                }
            }
            return 0;
        }

        public void CheckMask()
        {
            var x = Align();

            for(int i = 0; i < SeedArray.Length; i++)
            {
                if((ProgrammingChallenge2.Random._seedArray[((i - x - 1 + 55) % 55) + 1] != SeedArray[i]) && Mask[i])
                {
                    Console.WriteLine("******************************************************");
                    Console.WriteLine(ProgrammingChallenge2.Random._seedArray[((i - x - 1 + 55) % 55) + 1] + "|" + SeedArray[i]);
                    Dump();
                    ProgrammingChallenge2.Random.Dump(x);
                    System.Environment.FailFast("§df");
                }
            }
        }
    }
}