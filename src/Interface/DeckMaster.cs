using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NRT.Core;
using NRT.Flow;
using NRT.Resource;

namespace NRT.Interface;

public static class DeckMaster
{
    public static async Task Show()
    {
        string textToShow = "\t1) Create new deck\n\t2) Edit existing deck\n\t3) Delete existing deck\n\tq) Quit\n\nChoose action: ";

        App.ClearScreen();

        int userInput = Input.UserInput(textToShow, ceiling: 3, out bool quit, keyPhrase: "q");

        if (quit) return;

        switch (userInput)
        {
            case 1: await CreateDeck(); break;
            case 2: await EditDeck(); break;
            case 3: await DeleteDeck(); break;

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

        textToShow = "Enter a name for new deck ('q' to quit): ";
        deckTitle = Input.UserInput(textToShow, [..App.NOT_ALLOWED_NAMES, ..DeckProvider.GetDeckNames(toLower: true)], App.NOT_ALLOWED_CHARS, inverted: true, toLower: false);

        if (deckTitle.Equals("q")) return;
        
        App.ClearScreen();

        textToShow = "Do you want create 'Test' deck? (y/n): ";
        string userInput = Input.UserInput(textToShow, ["y", "n"]);

        if (userInput.Equals("y"))
            useSuperMemo = false;
        else
            useSuperMemo = true;

        Deck newDeck = new(deckTitle, useSuperMemo);
        Result<bool> writeResult = await Deck.WriteDeckAsync(newDeck);

        if (!writeResult.Success)
            throw writeResult.Exception;        
    }

    private static async Task EditDeck()
    {
        string textToShow;

        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();
        if (!result.Success && result.Exception is not FileNotFoundException)
            throw result.Exception;
        else if (!result.Success && result.Exception is FileNotFoundException)
        {
            Console.WriteLine("There's no created decks yet!");
            Console.ReadKey();

            return;
        }

        textToShow = string.Empty;

        string[] names = DeckProvider.GetDeckNames().ToArray();
        for (int i = 0; i < names.Length; i++)
            textToShow += $"\t{i + 1}) {names[i]}\n";

        textToShow += "\nChoose deck to edit ('q' to quit): ";

        int deckIndex = Input.UserInput(textToShow, ceiling: names.Length, out bool quit, keyPhrase: "q");

        if (quit) return;

        App.ClearScreen();

        Deck deckEdit = await DeckProvider.ProvideDeck(deckIndex - 1, DeckProvider.DeckPaths[deckIndex - 1], readFromFile: true);

        textToShow = "\t1) Add entry\n\t2) Edit entries\n\t3) Delete entries\n\tq) Quit\n\nChoose action: ";
        int userInput = Input.UserInput(textToShow, ceiling: 3, out quit, keyPhrase: "q");

        if (quit) return;

        switch (userInput)
        {
            case 1: await AddEntriesToDeck(deckEdit); break;
            case 2: break;
            case 3: break;

            default:
                throw new InvalidOperationException("Invalid input!");
        }
    }

    private static async Task DeleteDeck()
    {
        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();
        if (!result.Success && result.Exception is not FileNotFoundException)
            throw result.Exception;
        else if (!result.Success && result.Exception is FileNotFoundException)
        {
            Console.WriteLine("There's no created decks yet!");
            Console.ReadKey();

            return;
        }

        string textToShow = DeckBrowser.GetNumberedDeckList();
        textToShow += "\n\nChoose deck to delete ('q' to quit): ";

        int deckIndex = Input.UserInput(textToShow, ceiling: DeckProvider.DeckPaths.Length, out bool quit, keyPhrase: "q");

        if (quit) return;

        Console.Write("Are you sure? (press 'enter' to agree)");
        string userInput = App.ReadLine();

        if (userInput.Equals(""))
            await DeckProvider.DeleteDeck(deckIndex - 1, false);
    }

    private static async Task AddEntriesToDeck(Deck deck)
    {
        string entryTitle;
        string entryQuestion;

        string[] entryAnswerOption = [];
        string[] entryCorrectAnswers;

        bool isAnswerOrderImportant = false;

        string prefix;
        string textToShow;
        string userInput;

        while (true)
        {
            App.ClearScreen();

            Console.Write("Enter a title for entry (press 'enter' to skip, enter 'q' to quit): ");
            entryTitle = Console.ReadLine() ?? throw new InvalidOperationException("Console input is null!");
            
            if (entryTitle.Equals("q")) return;

            prefix = "Title is set!\n";

            App.ClearScreen(); 

            textToShow = prefix + "Enter a question for entry ('q' to quit): ";
            entryQuestion = Input.UserInput(textToShow, App.NOT_ALLOWED_NAMES, inverted: true, toLower: false);

            if (entryQuestion.Equals("q")) return;

            prefix += "Question is set!\n";

            App.ClearScreen();

            if (!deck.UseSuperMemo)
            {
                textToShow = prefix + "Enter an answer option for entry (press 'enter' to end): ";
                entryAnswerOption = Input.UserInput(textToShow).ToArray();

                prefix += "Answer options is set!\n";
            }

            App.ClearScreen();
            
            textToShow = prefix + "Enter a correct answer for entry (press 'enter' to end): ";
            entryCorrectAnswers = Input.UserInput(textToShow).ToArray();
        
            prefix += "Correct answer is set!\n";

            App.ClearScreen();

            if (entryCorrectAnswers.Length > 1)
            {
                textToShow = prefix + "Is answer order is important? (y/n) ";

                userInput = Input.UserInput(textToShow, ["y", "n"]);
                if (userInput.Equals("y"))
                    isAnswerOrderImportant = true;
            }
            else
                isAnswerOrderImportant = false;

            DeckEntry newEntry = new(entryTitle, entryQuestion, entryAnswerOption, entryCorrectAnswers, isAnswerOrderImportant);
            await deck.AddEntry(newEntry);

            App.ClearScreen();

            textToShow = "Create more? (y/n) ";
            userInput = Input.UserInput(textToShow, ["y", "n"]);

            if (userInput.Equals("n"))
                break;
        }
    }
}