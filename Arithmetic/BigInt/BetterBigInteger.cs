using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt; 

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;
    
    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        _signBit = isNegative == false ? 0 : 1;
        int len;
        for (len = digits.Length - 1; len >= 0; len--)
        {
           if (digits[len] != 0) break;
        }

        if (len == 0)
        {
            _data = null;
            _smallValue = digits[0];
        } else
        {
            _data = new uint[len + 1];
            Array.Copy(digits, 0, _data, 0, len + 1);    
        }
    }

    
    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false) : this(digits.ToArray(), isNegative) {}

    
    public BetterBigInteger(string value, int radix) {
        if (radix > 32 || radix < 2) {throw new NotSupportedException();}

        _signBit = value[0] == '-' ? 1 : 0;
        int startIndex = value[0] == '-' ? 1: 0;
        List<uint> digits = [0];

        for (int i = startIndex; i < value.Length; i++)
        {
            digits = BigMultiplyInt(digits, radix);
            digits = BigAddInt(digits, FromChar(value[i]));
        }

        if (digits.Count == 1)    
        {
            _smallValue = digits[0];
            _data = null;
        } else
        {
            _data = digits.ToArray();
        }
    }
    
    
    public ReadOnlySpan<uint> GetDigits()
    {
        return _data ?? [_smallValue];
    }
    
    public int CompareTo(IBigInteger? other)
    {
        if (other is null) return 1;

        if (this.IsNegative && !other.IsNegative) return -1;
        if (!this.IsNegative && other.IsNegative) return 1;
        int signBit = this.IsNegative == true ? -1 : 1;

        var OtherDigits = other.GetDigits();
        var ThisDigits = this.GetDigits();
        int signBitOther = other.IsNegative == true ? -1 : 1;

        if (OtherDigits.Length != ThisDigits.Length)
        {
            return ThisDigits.Length > OtherDigits.Length ? 1 * signBit : -1 * signBit;
        } else
        {
            for (int i = ThisDigits.Length - 1; i >= 0; i--)
            {
                if (ThisDigits[i] > OtherDigits[i])
                {
                    return 1 * signBit;   
                } 
                else if (ThisDigits[i] < OtherDigits[i])
                {
                    return -1 * signBit;
                }
            }
            return 0;
        }
    }

    public bool Equals(IBigInteger? other) {
        if (other is null) return false;
        if (CompareTo(other) == 0)
        {
            return true;
        }
        return false;
    }

    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);
    public override int GetHashCode() => throw new NotImplementedException();
    
    
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b) // поправить для 0 и 1 (сайн бит)
    {
        var ADigits = a.GetDigits();
        var BDigits = b.GetDigits();
        int len = Math.Max(ADigits.Length, BDigits.Length);
        ulong carry = 0;
        ulong temp;    
        List<uint> result = [];
        bool negative;
        int AsignBit = a.IsNegative == true ? -1 : 1;
        int BsignBit = b.IsNegative == true ? -1 : 1;

        if (AsignBit * BsignBit == 1)
        {
            negative = AsignBit == -1;
            for (int i = 0; i < len; ++i)
            {       
                uint ADigit = i < ADigits.Length ? ADigits[i] : 0;
                uint BDigit = i < BDigits.Length ? BDigits[i] : 0;

                temp = (ulong)((ulong)ADigit + (ulong)BDigit + carry);
                result.Add((uint)temp);
                carry = temp >> 32;
            } 
            if (carry != 0)
            {
                result.Add((uint)carry);
            }
        } else {
            int cmp = a.CompareAbs(b); // узнали кто больше по модулю
            negative = FigureSign(cmp, AsignBit, BsignBit) == 1;
            
            for (int i = 0; i < len; ++i)
            {       
                long ADigit = (i < ADigits.Length ? ADigits[i] : 0);
                long BDigit = (i < BDigits.Length ? BDigits[i] : 0);

                temp = (ulong)(ADigit * cmp + BDigit * b.CompareAbs(a) + (long)carry); // чо с эти сделать ептить ааааа

                result.Add((uint)temp);
                carry = temp >> 32;
            } 
            if (carry != 0)   
                result.Add((uint)carry);     
        }

        return new BetterBigInteger(result.ToArray(), negative);
    }
    private int CompareAbs(BetterBigInteger other) // работает!
    {
        if (_signBit == 0 && other._signBit == 1)
        {
            return CompareTo(-other);
        }
        if (_signBit == 1 && other._signBit == 0)
        {
            return (-this).CompareTo(other);
        }
        if (_signBit == 1 && other._signBit == 1)
        {
            return (-this).CompareTo(-other);
        }
        else 
        {
            return this.CompareTo(other);
        } 
    }

    private static int FigureSign(int cmpAbs, int Asign, int Bsign)
    {
        if (cmpAbs == 1)  // a > b by modulo
        {
            return Asign == 1 ? 1 : 0;
        } else
        {
            return Bsign == 1 ? 1 : 0;
        }
    }

    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b)
    {
        return a + (-b);
    }
    public static BetterBigInteger operator -(BetterBigInteger a) {
        uint[] digits = a._data ?? [a._smallValue];
        return new BetterBigInteger(digits, isNegative: a._signBit == 1 ? false : true);
    }

    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    
    
    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    => throw new NotImplementedException("Умножение делегируется стратегии, выбирать необходимо в зависимости от размеров чисел");
    
    public static BetterBigInteger operator ~(BetterBigInteger a) => throw new NotImplementedException();
    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b) => throw new NotImplementedException();
    public static BetterBigInteger operator <<(BetterBigInteger a, int shift) => throw new NotImplementedException();
    public static BetterBigInteger operator >> (BetterBigInteger a, int shift) => throw new NotImplementedException();
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    public override string ToString() => ToString(10);
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 32) {throw new NotSupportedException();}
        uint[] digits = this.GetDigits().ToArray();

        List<char> result = [];

        while (!(digits.Length == 1 && digits[0] == 0)) 
        {
            List<uint> tempNum = [];
            ulong remain = 0;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                ulong temp = digits[i] + (remain << 32);
                tempNum.Add((uint)(temp / (ulong)radix));
                remain = temp % (ulong)radix;
            }
            result.Add(ToChar(remain));
            tempNum.Reverse();
            while (tempNum.Count > 1 && tempNum[tempNum.Count - 1] == 0)
                tempNum.RemoveAt(tempNum.Count - 1);
            digits = tempNum.ToArray();
        
        }

        result.Reverse();
        if (IsNegative) {result.Insert(0, '-');}
        return new string(result.ToArray()) ?? "0"; // интересно, а так можно?
    }

    private static char ToChar(ulong num)
    {
        return num > 9 ? (char)('A' +  num - 10) : (char)('0' + num);
    }

    private static int FromChar(char c)
    {
        Console.WriteLine(c);
        return char.IsDigit(c) ? (c - '0') : (c - 'A' + 10);
    }

    private static List<uint> BigMultiplyInt(List<uint> digits, int num)
    {
        if (num == 0) {return new List<uint>{0};}

        List<uint> result = new(digits.Count);
        ulong carry = 0;

        for (int i = 0; i < digits.Count; ++i)
        {
            ulong product = carry + (ulong)digits[i] * (ulong)num;
            result.Add((uint)product);
            carry = product >> 32;
        }
        if (carry != 0)
        {
            result.Add((uint)(carry));
        }
        return result;
    }

    private static List<uint> BigAddInt(List<uint> digits, int num)
    {
        ulong carry;
        List<uint> result = new(digits.Count + 1);
        ulong sum = (ulong)(digits[0] + num);
        result.Add((uint)sum);
        carry = sum >> 32;

        for (int i = 1; i < digits.Count; ++i)
        {
            sum = digits[i] + carry;
            result.Add((uint)sum);
            carry = sum >> 32;
        }
        if (carry != 0)
        {
            result.Add((uint)carry);
        }

        return result;
    }
} 