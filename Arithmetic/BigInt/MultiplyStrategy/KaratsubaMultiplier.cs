using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var ADigits = a.GetDigits();
        var BDigits = b.GetDigits();
        int lenMax = Math.Max(ADigits.Length, BDigits.Length);

        if (lenMax == 0)
        {
            return new BetterBigInteger("0", 10);
        } 

        // надо еще добавить заполнение нулями до одинаковой длины
        if (lenMax == 1)
        {
            return a * b;
        }

        int half1 = lenMax / 2;
        int half2 = lenMax - half1;

        BetterBigInteger a0 = new(ADigits[..half1].ToArray(), a.IsNegative);
        BetterBigInteger a1 = new(ADigits[(half1 + 1)..].ToArray(), a.IsNegative);

        BetterBigInteger b0 = new(BDigits[..half1].ToArray(), b.IsNegative);
        BetterBigInteger b1 = new(BDigits[(half1 + 1)..].ToArray(), b.IsNegative);

        BetterBigInteger D1 = Multiply(a0, b0);
        BetterBigInteger D3 = Multiply(a1, b1);
        BetterBigInteger D2 = Multiply(a0 + a1, b0 + b1);
        // D1 + (D2 - D1 - D3) * x + D3 * x**2 => {D1, D2-D1-D3, D3}
        return // нажо как-то результат записать а как 

    } 


} 

