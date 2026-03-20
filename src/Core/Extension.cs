using System;

namespace NRT.Core;

public static class Extension
{
    private static readonly Random random = new();

    public static string Display(this ELogLevel level) => level.ToString().ToUpper();

    public static void Shuffle<T>(this T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            (array[j], array[i]) = (array[i], array[j]);
        }
    }
}