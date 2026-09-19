using System;
using System.Linq;

namespace Accountify;

public static class Function
{
    private static readonly Random Random = new();

    public static int NextInt(int a, int b)
    {
        if (a == b) return a;
        return a + Random.Next(b - a);
    }

    public static string GenerateRandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}