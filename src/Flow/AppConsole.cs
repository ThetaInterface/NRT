#pragma warning disable CS0162

using System;

namespace NRT.Flow;

public static partial class App
{
    public static void ClearScreen()
    {
        if (!DEBUG)
            Console.Clear();
    }

    public static string ReadLine() => Console.ReadLine() ?? throw new InvalidOperationException("Console is null!");
    
}

#pragma warning restore CS0162