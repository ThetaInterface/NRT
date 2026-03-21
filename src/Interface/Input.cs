using System;
using System.Linq;
using System.Collections.Generic;

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

    public static int UserInput(string text, int ceiling, out bool isKeyPhrase, int floor = 1, string? keyPhrase = null, bool toLower = true)
    {
        while (true)
        {
            App.ClearScreen();
            Console.Write(text);

            string userInput = App.ReadLine();

            if (toLower)
                userInput = userInput.ToLower();

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

    public static string UserInput(string text, string[] vocabulary, char[]? notAllowedSymbols = null, bool inverted = false, bool toLower = true)
    {
        while (true)
        {
            App.ClearScreen();
            Console.Write(text);

            string userInput = App.ReadLine();

            if (toLower)
                userInput = userInput.ToLower();

            if (notAllowedSymbols != null && userInput.IndexOfAny(notAllowedSymbols) != -1)
            {
                Console.WriteLine("Text contains not allowed symbol!");
                Console.ReadKey();

                continue;
            }

            if (!inverted && vocabulary.Contains(userInput.ToLower()))
                return userInput;
            else if (inverted && !vocabulary.Contains(userInput.ToLower()))
                return userInput;
            else
            {
                Console.WriteLine("This text is not allowed!");
                Console.ReadKey();
            }
        }
    }

    public static IEnumerable<string> UserInput(string text, string phraseToQuit = "")
    {
        while (true)
        {
            App.ClearScreen();
            Console.Write(text);

            string input = App.ReadLine();

            if (!input.Equals(phraseToQuit))
                yield return input;
            else
                break;
        }
    }
}