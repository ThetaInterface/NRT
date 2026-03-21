using System;

namespace NRT.Core;

public static class Extension
{
    private static readonly Random random = new();

    public static string Display(this ELogLevel level) => level.ToString().ToUpper();

    public static T[] Shuffle<T>(this T[] array)
    {
        T[] newArray = array.Clone() as T[] ?? throw new InvalidOperationException("Can not create new array to shuffle it!");

        for (int i = newArray.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            (newArray[j], newArray[i]) = (newArray[i], newArray[j]);
        }

        return newArray;
    }
}