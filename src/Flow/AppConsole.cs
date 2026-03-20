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
}

#pragma warning restore CS0162