using System.Threading.Tasks;

using NRT.Flow;
using NRT.Resource;
using NRT.Core;
using System.IO;
using System;
using System.Linq;

namespace NRT.Interface;

public static class DeckMaster
{
    internal static readonly string[] NOT_ALLOWED_NAMES = [
        "q",
    ];

    public static async Task Show()
    {
        string textToShow = "\t1) Create new deck\n\t2) Edit existing deck\n\tq) Quit";

        App.ClearScreen();

        int userInput = Input.UserInput(textToShow, ceiling: 2, out bool quit, keyPhrase: "q");

        if (quit) return;

        switch (userInput)
        {
            case 1: await CreateDeck(); break;
            case 2: await EditDeck(); break;

            default:
                throw new InvalidOperationException("Invalid input!");
        }
    }

    private static async Task CreateDeck()
    {
        string deckTitle;
        bool useSuperMemo;

        string textToShow;

        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();
        if (!result.Success && result.Exception is not FileNotFoundException)
            throw result.Exception;

        textToShow = "Enter a name for new deck: ";
        deckTitle = Input.UserInput(textToShow, NOT_ALLOWED_NAMES, Path.GetInvalidFileNameChars(), inverted: true);
        
        App.ClearScreen();

        textToShow = "Do you want create 'Test' deck? (y/n): ";
        string userInput = Input.UserInput(textToShow, ["y", "n"]);

        if (userInput.Equals("y"))
            useSuperMemo = true;
        else
            useSuperMemo = false;

        Deck newDeck = new(deckTitle, useSuperMemo);
        await Deck.WriteDeckAsync(newDeck);
    }

    private static async Task EditDeck()
    {
        string textToShow;

        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();
        if (!result.Success && result.Exception is not FileNotFoundException)
            throw result.Exception;
        else if (!result.Success && result.Exception is  FileNotFoundException)
        {
            Console.WriteLine("There's no created decks yet!");
            Console.ReadKey();

            return;
        }

        textToShow = string.Empty;

        string[] names = DeckProvider.GetDeckNames().ToArray();
        for (int i = 0; i < names.Length; i++)
            textToShow += $"\t{i + 1}) {names[i]}\n";

        textToShow += "\nChoose deck to edit: ";

        int deckIndex = Input.UserInput(textToShow, ceiling: names.Length, out bool quit, keyPhrase: "q");

        if (quit) return;

        App.ClearScreen();

        Deck deckEdit = await DeckProvider.ProvideDeck(deckIndex - 1, DeckProvider.DeckPaths[deckIndex - 1], ConfigProvider.AppConfig.LiteMode);

        textToShow = "\t1) Add entry\n\t2) Edit entries\n\t3) Delete entries\n\tq) Quit";
        int userInput = Input.UserInput(textToShow, ceiling: 3, out quit, keyPhrase: "q");

        if (quit) return;

        switch (userInput)
        {
            case 1: break;
            case 2: break;
            case 3: break;

            default:
                throw new InvalidOperationException("Invalid input!");
        }
    }
}