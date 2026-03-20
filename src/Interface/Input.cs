using System;
using System.Linq;

using NRT.Flow;

namespace NRT.Interface;

public static class Input
{
    public static char UserInput(string text, char[] allowedInputChars)
    {
        while (true)
        {
            App.ClearScreen();
            Console.Write(text);

            char input = Console.ReadKey().KeyChar;

            if (allowedInputChars.Contains(input))
                return input;
        }
    }

    public static int UserInput(string text, int ceiling, out bool isKeyPhrase, int floor = 1, string? keyPhrase = null)
    {
        while (true)
        {
            App.ClearScreen();
            Console.Write(text);

            string userInput = Console.ReadLine() ?? "";

            if (keyPhrase != null && keyPhrase.Equals(userInput))
            {
                isKeyPhrase = true;
                
                return floor;
            }

            if (int.TryParse(userInput, out int result))
                if (result >= floor && result <= ceiling)
                {
                    isKeyPhrase = false;

                    return result;
                }
        }
    }

    public static string UserInput(string text, string[] vocabulary)
    {
        while (true)
        {
            Console.Clear();
            Console.Write(text);

            string input = Console.ReadLine() ?? "";

            if (vocabulary.Contains(input))
                return input;
        }
    }
}