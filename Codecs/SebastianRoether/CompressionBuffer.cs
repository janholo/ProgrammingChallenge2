namespace ProgrammingChallenge2.Codecs.SebastianRoether
{
    using System;
    using System.Linq;

    public class CompressionBuffer
    {
        private const int MBIG = Int32.MaxValue;
        private int inext;
        private int inextp;
        private int?[] SeedArray = new int?[56];
        private bool debug;

        public CompressionBuffer(bool debug)
        {
            inext = 0;
            inextp = 21;
            this.debug = debug;
        }

        internal CompressionBuffer(int inext, int inextp, int?[] buffer, bool debug)
        {
            this.inext = inext;
            this.inextp = inextp;
            this.SeedArray = buffer;
            this.debug = debug;
        }

        public CompressionBuffer Clone()
        {
            var copyArray = new int?[SeedArray.Length];
            Array.Copy(SeedArray,copyArray, SeedArray.Length);
            return new CompressionBuffer(inext, inextp, copyArray, debug);
        }

        //
        // Package Private Methods
        //

        /*====================================Sample====================================
        **Action: Return a new random number [0..1) and reSeed the Seed array.
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        protected virtual double? Sample()
        {
            //Including this division at the end gives us significantly improved
            //random number distribution.
            return (InternalSample() * (1.0 / MBIG));
        }

        private int? InternalSample()
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            if(SeedArray[locINext] == null ||SeedArray[locINextp] == null)
            {
                return null;
            }

            retVal = SeedArray[locINext].Value - SeedArray[locINextp].Value;

            if (retVal == MBIG) retVal--;
            if (retVal < 0) retVal += MBIG;

            SeedArray[locINext] = retVal;

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        public void observe(double value)
        {
            internalObserve((int)(value * MBIG));
        }

        public void miss()
        {
            internalObserve(null);
        }

        public int P()
        {
            return SeedArray.Count(x => x == null);
        }

        private void internalObserve(int? value)
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            if(SeedArray[locINext] == null || SeedArray[locINextp] == null)
            {
                SeedArray[locINext] = value;
                inext = locINext;
                inextp = locINextp;
            }
            else
            {
                retVal = SeedArray[locINext].Value - SeedArray[locINextp].Value;

                if (retVal == MBIG) retVal--;
                if (retVal < 0) retVal += MBIG;

                if((retVal != value) && value != null)
                {
                    if(debug)
                    {
                        if(Math.Abs((long)retVal-(long)value.Value) != 0)
                        {
                            Console.WriteLine("actual value " + retVal + " does not match observation " + value);
                        }
                        else
                        {
                            Console.WriteLine("Good enough");
                        }
                    }

                    SeedArray[locINext] = value;
                    //throw new Exception("actual value " + retVal + " does not match observation " + value);
                }
                else
                {
                    SeedArray[locINext] = retVal;
                }

                inext = locINext;
                inextp = locINextp;
            }
        }

        /*=====================================Next=====================================
        **Returns: An int [0..Int32.MaxValue)
        **Arguments: None
        **Exceptions: None.
        ==============================================================================*/
        public virtual int? Next()
        {
            return InternalSample();
        }

        private double? GetSampleForLargeRange()
        {
            // The distribution of double value returned by Sample 
            // is not distributed well enough for a large range.
            // If we use Sample for a range [Int32.MinValue..Int32.MaxValue)
            // We will end up getting even numbers only.

            var result = InternalSample();
            if(result == null)
            {
                return null;
            }

            // Note we can't use addition here. The distribution will be bad if we do that.
            bool negative = (InternalSample() % 2 == 0) ? true : false;  // decide the sign based on second sample
            if (negative)
            {
                result = -result;
            }

            double d = result.Value;
            d += (Int32.MaxValue - 1); // get a number in range [0 .. 2 * Int32MaxValue - 1)
            d /= 2 * (uint)Int32.MaxValue - 1;
            return d;
        }


        /*=====================================Next=====================================
        **Returns: An int [minvalue..maxvalue)
        **Arguments: minValue -- the least legal value for the Random number.
        **           maxValue -- One greater than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int? Next(int minValue, int maxValue)
        {
            long range = (long)maxValue - minValue;
            if (range <= (long)Int32.MaxValue)
            {
                return ((int?)(Sample() * range) + minValue);
            }
            else
            {
                return (int?)((long?)(GetSampleForLargeRange() * range) + minValue);
            }
        }


        /*=====================================Next=====================================
        **Returns: An int [0..maxValue)
        **Arguments: maxValue -- One more than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int? Next(int maxValue)
        {
            return (int?)(Sample() * maxValue);
        }


        /*=====================================Next=====================================
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        public virtual double? NextDouble()
        {
            return Sample();
        }
    }
}