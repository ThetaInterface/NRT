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
            {
                if (result >= floor && result <= ceiling)
                {
                    isKeyPhrase = false;

                    return result;
                }
                else
                {
                    Console.WriteLine($"Enter a number between {floor} and {ceiling}!");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Not a number!");
                Console.ReadKey();
            }
        }
    }

    public static string UserInput(string text, string[] vocabulary, char[]? notAllowedSymbols = null, bool inverted = false)
    {
        while (true)
        {
            Console.Clear();
            Console.Write(text);

            string input = Console.ReadLine() ?? "";

            if (notAllowedSymbols != null && input.IndexOfAny(notAllowedSymbols) != -1)
            {
                Console.WriteLine("Text contains not allowed symbol!");
                Console.ReadKey();

                continue;
            }

            if (!inverted && vocabulary.Contains(input))
                return input;
            else if (inverted && !vocabulary.Contains(input))
                return input;
            else
            {
                Console.WriteLine("This word is not allowed!");
                Console.ReadKey();
            }
        }
    }
}