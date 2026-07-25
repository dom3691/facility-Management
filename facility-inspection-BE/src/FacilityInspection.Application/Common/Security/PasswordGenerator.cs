using System.Security.Cryptography;

namespace FacilityInspection.Application.Common.Security;

/// <summary>Generates cryptographically random passwords that satisfy the Identity policy.</summary>
public static class PasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%&*?";

    public static string Generate(int length = 16)
    {
        if (length < 12)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 12.");
        }

        var chars = new char[length];
        chars[0] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
        chars[1] = Lower[RandomNumberGenerator.GetInt32(Lower.Length)];
        chars[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
        chars[3] = Symbols[RandomNumberGenerator.GetInt32(Symbols.Length)];

        var all = Upper + Lower + Digits + Symbols;
        for (var i = 4; i < length; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Fisher–Yates shuffle so required character positions are not predictable.
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
