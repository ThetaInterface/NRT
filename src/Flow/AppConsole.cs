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
    
    public static void Write(string? text, bool nextLine = false) 
    {   
        Console.Write(SPACE);
        Console.SetCursorPosition(0, 0);

        if (nextLine)
            Console.WriteLine(text);
        else
            Console.Write(text);  
    }
}

#pragma warning restore CS0162