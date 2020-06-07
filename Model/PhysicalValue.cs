using System;
using System.Globalization;

namespace ProgrammingChallenge2.Model
{
    public class PhysicalValue
    {
        public PhysicalValue(double value, string unit)
        {
            Value = value;
            Unit = unit;
        }

        public double Value { get; }
        public string Unit { get; }

        public override string ToString()
        {
            return FormattableString.Invariant($"{Value:F3}{Unit}");
        }
    }
}