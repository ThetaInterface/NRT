using System;
using System.Linq;
using System.Collections.Generic;

using NRT.Flow;
using NRT.Core;

namespace NRT.Interface;

public static class Input
{
    public static char UserInput(string text, char[] allowedInputChars)
    {
        while (true)
        {
            App.ClearScreen();
            App.Write(text);

            char input = App.ReadKey();

            if (allowedInputChars.Contains(input))
                return input;
        }
    }

    public static int UserInput(string text, 
        int ceiling, 
        out string keyPhrase, 
        int floor = 1, 
        string[]? keyPhrases = null, 
        string defaultKeyPhrase = "", 
        bool toLower = true)
    {
        while (true)
        {
            App.ClearScreen();
            App.Write(text);

            string userInput = App.ReadLine();

            if (toLower)
                userInput = userInput.ToLower();

            if (keyPhrases != null && keyPhrases.Contains(userInput))
            {
                keyPhrase = userInput;
                
                return floor - 1;
            }

            if (int.TryParse(userInput, out int result))
            {
                if (result >= floor && result <= ceiling)
                {
                    keyPhrase = defaultKeyPhrase;

                    return result;
                }
                else
                {
                    App.Write($"Enter a number between {floor} and {ceiling}!", nextLine: true);
                    App.ReadKey();
                }
            }
            else
            {
                App.Write("Not a number!", nextLine: true);
                App.ReadKey();
            }
        }
    }

    public static string UserInput(string text, string[] vocabulary, char[]? notAllowedSymbols = null, bool inverted = false, bool toLower = true)
    {
        while (true)
        {
            App.ClearScreen();
            App.Write(text);

            string userInput = App.ReadLine();

            if (toLower)
                userInput = userInput.ToLower();

            if (notAllowedSymbols != null && userInput.IndexOfAny(notAllowedSymbols) != -1)
            {
                App.Write("Text contains not allowed symbol!", nextLine: true);
                App.ReadKey();

                continue;
            }

            if (!inverted && vocabulary.Contains(userInput.ToLower()))
                return userInput;
            else if (inverted && !vocabulary.Contains(userInput.ToLower()))
                return userInput;
            else
            {
                App.Write("This text is not allowed!", nextLine: true);
                App.ReadKey();
            }
        }
    }

    public static string[] UserInput(string text, out bool quit, string phraseToQuit = "")
    {
        while (true)
        {
            App.ClearScreen();
            App.Write(text);

            string input = App.ReadLine();

            if (input != null && input.Length > 0)
            {
                if (input.Equals(phraseToQuit))
                {
                    quit = true;

                    return [];
                }

                quit = false;

                return input.Split(ConfigProvider.AppConfig.SeparatorSymbols, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }
    }
}