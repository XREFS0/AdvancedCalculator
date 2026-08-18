using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;

namespace AdvancedCalculator.Application.Services;

public class ProgrammerService : IProgrammerService
{
    public long MaskToBitSize(long value, BitSize bitSize)
    {
        return bitSize switch
        {
            BitSize.Byte => (byte)value,
            BitSize.Word => (ushort)value,
            BitSize.Dword => (uint)value,
            BitSize.Qword => value,
            _ => value
        };
    }

    public long EvaluateBitwise(long left, string op, long right, BitSize bitSize)
    {
        long l = MaskToBitSize(left, bitSize);
        long r = MaskToBitSize(right, bitSize);

        long res = op.ToUpperInvariant() switch
        {
            "AND" or "&" => l & r,
            "OR" or "|" => l | r,
            "XOR" or "^" => l ^ r,
            "NAND" => ~(l & r),
            "NOR" => ~(l | r),
            "XNOR" => ~(l ^ r),
            _ => throw new ArgumentException($"Invalid bitwise operator: {op}")
        };

        return MaskToBitSize(res, bitSize);
    }

    public long PerformUnaryOp(string op, long value, BitSize bitSize)
    {
        long val = MaskToBitSize(value, bitSize);
        long res = op.ToUpperInvariant() switch
        {
            "NOT" or "~" => ~val,
            "NEG" or "+/-" => -val,
            _ => val
        };
        return MaskToBitSize(res, bitSize);
    }

    public long ShiftLeft(long value, int count, BitSize bitSize)
    {
        long val = MaskToBitSize(value, bitSize);
        return MaskToBitSize(val << count, bitSize);
    }

    public long ShiftRight(long value, int count, BitSize bitSize)
    {
        long val = MaskToBitSize(value, bitSize);
        // Logical right shift for unsigned mask
        ulong uVal = (ulong)val;
        ulong res = uVal >> count;
        return MaskToBitSize((long)res, bitSize);
    }

    public string FormatNumber(long value, NumberBase numberBase, BitSize bitSize)
    {
        long masked = MaskToBitSize(value, bitSize);
        ulong unsignedVal = (ulong)masked;

        return numberBase switch
        {
            NumberBase.Hexadecimal => unsignedVal.ToString("X"),
            NumberBase.Decimal => bitSize switch
            {
                BitSize.Byte => ((sbyte)masked).ToString(),
                BitSize.Word => ((short)masked).ToString(),
                BitSize.Dword => ((int)masked).ToString(),
                _ => masked.ToString()
            },
            NumberBase.Octal => Convert.ToString((long)unsignedVal, 8),
            NumberBase.Binary => FormatBinary(unsignedVal, bitSize),
            _ => masked.ToString()
        };
    }

    private static string FormatBinary(ulong value, BitSize bitSize)
    {
        int bits = (int)bitSize;
        string raw = Convert.ToString((long)value, 2).PadLeft(bits, '0');
        if (raw.Length > bits)
            raw = raw[^bits..];

        // Format in 4-bit nibbles separated by space: e.g. 0000 1101
        var chunks = new List<string>();
        for (int i = 0; i < raw.Length; i += 4)
        {
            int len = Math.Min(4, raw.Length - i);
            chunks.Add(raw.Substring(i, len));
        }
        return string.Join(" ", chunks);
    }

    public bool TryParse(string input, NumberBase numberBase, BitSize bitSize, out long value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        string clean = input.Replace(" ", "").Trim();

        try
        {
            switch (numberBase)
            {
                case NumberBase.Hexadecimal:
                    value = MaskToBitSize(Convert.ToInt64(clean, 16), bitSize);
                    return true;
                case NumberBase.Decimal:
                    value = MaskToBitSize(long.Parse(clean), bitSize);
                    return true;
                case NumberBase.Octal:
                    value = MaskToBitSize(Convert.ToInt64(clean, 8), bitSize);
                    return true;
                case NumberBase.Binary:
                    value = MaskToBitSize(Convert.ToInt64(clean, 2), bitSize);
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
}
