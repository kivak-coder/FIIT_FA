using System.Runtime.CompilerServices;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        bool negative;
        int aSign = a.IsNegative == true ? -1 : 1;
        int Bsign = b.IsNegative == true ? -1 : 1;
        negative = aSign * Bsign != 1;
        var Adigits = a.GetDigits();
        var BDigits = b.GetDigits();
        uint[] result = new uint[Adigits.Length + BDigits.Length];
        Console.WriteLine(result);

        for (int i = 0; i < Adigits.Length; ++i)
        {
            ulong carry = 0;

            for (int j = 0; j < BDigits.Length; ++j)
            {
                ulong product = (ulong)Adigits[i] * (ulong)BDigits[j] + carry + result[i + j];
                result[i + j] = (uint)product;
                carry = product >> 32;
            }

            if (carry != 0)
            {
                result[i + BDigits.Length] = (uint)carry;
            }
        }
        return new BetterBigInteger(result.ToArray(), negative);
    }
}