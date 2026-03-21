using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using NRT.Core;
using NRT.Flow;
using NRT.Resource;

namespace NRT.Interface;

public static class DeckBrowser
{
    private static List<int> entryCounts = [];
    private static int? overallReviewedCount = null;
    private static string cardsText = string.Empty;

    public static async Task Show()
    {
        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();

        if (!result.Success)
        {
            Console.WriteLine("There's no decks created yet!\n\nPress any button to return...");
            Console.ReadKey();

            return;
        }

        if (!ConfigProvider.AppConfig.LiteMode)
        {
            await DeckProvider.LoadDecks();
            await DeckProvider.UpdateDecks();

            entryCounts = DeckProvider.GetEntryCounts().ToList();

            overallReviewedCount = 0;
            foreach (var deck in DeckProvider.Decks)
                overallReviewedCount += deck.ReviewCount;
        }

        string text = "\t1) Browser mode\n\t2) Review mode\n\nChoose mode ('q' to return): ";
        int userInput = Input.UserInput(text, ceiling: 2, out bool quit, keyPhrase: "q");

        if (quit) return;
        
        switch (userInput)
        {
            case 1: await BrowseMode(); break;
            case 2: await ReviewMode(); break;
            
            default:
                throw new InvalidOperationException("Invalid input!");
        }
    }

    public static string GetNumberedDeckList()
    {
        string text = string.Empty;

        string[] deckNames = DeckProvider.GetDeckNames().ToArray();
        for (int i = 0; i < deckNames.Length; i++)
            text += $"\t{i + 1}) " + deckNames[i] + "\n";

        return text;
    }

    private static async Task BrowseMode()
    {
        App.ClearScreen();
        
        string text = GetNumberedDeckList();
        text += "\nChoose deck to browse (enter 'q' to return): ";

        int deckIndex = Input.UserInput(text, DeckProvider.DeckPaths.Length, out bool quit, keyPhrase: "q");

        if (quit) return;

        App.ClearScreen();

        Deck deck = await DeckProvider.ProvideDeck(deckIndex - 1, DeckProvider.DeckPaths[deckIndex - 1], ConfigProvider.AppConfig.LiteMode);

        text = string.Empty;
        cardsText = string.Empty;
        for (int i = 0; i < deck.Entries.Count; i++)
        {
            cardsText += "------------------------------\n";
            cardsText += deck.Entries[i].ToString(i + 1);
        }

        cardsText += "------------------------------\n";

        text += "\n\ts) Search mode\n\tc) Sort cards\n\te) Edit\n\tq) Quit\n\nChoose action: ";

        string actionCode = Input.UserInput(cardsText + text, ["s", "c", "e", "q"]);
        switch (actionCode)
        {


            case "q":
                return;

            default:
                throw new InvalidOperationException("Invalid input!");
        }
    }

    private static async Task ReviewMode()
    {
        App.ClearScreen();

        string text = string.Empty;
        string[] deckNames = DeckProvider.GetDeckNames().ToArray();
        for (int i = 0; i < deckNames.Length; i++)
        {
            text += $"\t{i + 1}) " + deckNames[i];

            if (entryCounts.Count > 0)
                text += $" ({entryCounts[i]} today)";

            text += "\n";
        }

        text += $"{(overallReviewedCount != null ? $"\n\tToday review {overallReviewedCount} cards\n\n" : string.Empty)}" + 
            "Choose deck to start (enter 'q' to return): ";

        int deckIndex = Input.UserInput(text, DeckProvider.DeckPaths.Length, out bool quit, keyPhrase: "q");

        if (quit) return;

        Deck deck = await DeckProvider.ProvideDeck(deckIndex - 1, DeckProvider.DeckPaths[deckIndex - 1], ConfigProvider.AppConfig.LiteMode);
        DeckEntry[] entries = deck.GetEntriesByDate(DateTime.Now).ToArray();

        foreach (DeckEntry entry in entries)
        {
            bool end;

            if (deck.UseSuperMemo)
                ShowDeckEntryWithSM2(entry, out end);
            else
                ShowDeckEntryWithoutSM2(entry, out end);

            if (end) break;
        }

        Result<bool> writeResult = await Deck.WriteDeckAsync(deck);

        if (!writeResult.Success)
            throw writeResult.Exception;    

        entries = deck.GetEntriesByDate(DateTime.Now).ToArray();

        App.ClearScreen();

        if (entries.Length <= 0) 
        {
            Console.WriteLine("All cards reviewed!\nPress any button to return...");
            Console.ReadKey();
        }
    }

    private static void ShowDeckEntryWithSM2(DeckEntry entry, out bool wantToEnd)
    {
        App.ClearScreen();
        
        string problemText = $"{(entry.Title.Length <= 0 ? string.Empty : $"{entry.Title}\n")}" +
            $"{entry.Question}\n\n";
        
        problemText += "Press enter to swap ('q' to return | 's' to skip): ";

        wantToEnd = false;

        App.ClearScreen();
        Console.Write(problemText);

        string userInput = App.ReadLine();

        if (userInput.Equals("q"))
        {
            wantToEnd = true;

            return;
        }
        else if (userInput.Equals("s"))
            return;

        string rateText = $"\n\nAnswer is: {entry.GetCorrectAnswer()}\n\t" + 
            "1) Dont remember\n\t" + 
            "2) Saw answer and remembered\n\t" + 
            "3) Remembered, but with big effort\n\t" +
            "4) Remembered, but with lite effort\n\t" +
            "5) Remember\n\t" +
            "6) Too easy\n" +
            "Rate yourself: ";

        int quality = Input.UserInput(problemText + rateText, ceiling: 6, out _);

        entry.Answer(quality - 1);
    }

    private static void ShowDeckEntryWithoutSM2(DeckEntry entry, out bool wantToEnd)
    {
        App.ClearScreen();

        string[] answers = entry.AnswersOptions;
        string[] answeroptions = answers.Shuffle();
        
        string text = $"{(entry.Title.Length <= 0 ? string.Empty : $"{entry.Title}\n")}" +
            $"{entry.Question}\n\n";

        for (int i = 0; i < answeroptions.Length; i++)
            text += $"\t{i + 1}) {answeroptions[i]}\n";
        
        text += "\nChoose right answers ('q' to return | 's' to skip): ";


        wantToEnd = false;

        while (true)
        {
            App.ClearScreen();
            Console.Write(text);

            string userInput = App.ReadLine();

            if (userInput.Equals("q"))
            {
                wantToEnd = true;

                return;
            }
            else if (userInput.Equals("s"))
                return;

            Console.Write("Are you sure ('enter' to agree)?");

            if (App.ReadLine().Length <= 0)
            {
                char separator = ConfigProvider.AppConfig.SeparatorSymbol;
                string[] answerParts = userInput.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                string answer = userInput;
                foreach (var part in answerParts)
                {
                    if (int.TryParse(part, out int index))
                        if (index - 1 < answeroptions.Length)
                            answer = answer.Replace(part, answeroptions[index - 1]);
                }

                bool isCorrect = entry.Answer(answer);

                Console.WriteLine("\n" + (isCorrect ? "Correct!" : "Mistake!"));
                App.ReadLine();

                break;
            }
        }
    }
}