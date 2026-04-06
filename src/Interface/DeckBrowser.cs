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
    internal const string QUIT_PHRASE = "q";
    internal const string SEARCH_PHRASE = "s";
    internal const string SORT_PHRASE = "c";

    private static List<int> entryCounts = [];
    private static int? overallReviewedCount = null;

    public static async Task Show()
    {
        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();

        if (!result.Success)
        {
            App.Write("There's no decks created yet!\n\nPress any button to return...", nextLine: true);
            App.ReadKey();

            return;
        }

        while (true)
        {
            await UpdateDeckInfo();

            string text = "\t1) Browser mode\n\t2) Review mode\n\nChoose mode ('q' to return): ";
            int userInput = Input.UserInput(text, ceiling: 2, out string quit, keyPhrases: [QUIT_PHRASE]);

            if (quit.Equals(QUIT_PHRASE)) break;
            
            switch (userInput)
            {
                case 1: await BrowseMode(); break;
                case 2: await ReviewMode(); break;
                
                default:
                    throw new InvalidOperationException("Invalid input!");
            }
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

    public static string ComposeEntryList(List<DeckEntry> entries)
    {
        string composed = string.Empty;
        for (int i = 0; i < entries.Count; i++)
        {
            composed += "------------------------------\n";
            composed += entries[i].ToString(i + 1);
        }

        composed += "------------------------------\n";

        return composed;
    }

    private static async Task BrowseMode()
    {
        App.ClearScreen();
        
        while (true)
        {
            string text = GetNumberedDeckList();
            text += "\nChoose deck to browse (enter 'q' to return): ";

            int deckIndex = Input.UserInput(text, DeckProvider.DeckPaths.Length, out string quit, keyPhrases: [QUIT_PHRASE]);

            if (quit.Equals(QUIT_PHRASE)) break;

            while (true)
            {
                quit = "";

                App.ClearScreen();

                Deck deck = await DeckProvider.ProvideDeck(deckIndex - 1, DeckProvider.DeckPaths[deckIndex - 1], ConfigProvider.AppConfig.LiteMode);

                string cardsText = ComposeEntryList(deck.Entries);
                text = "\n\ts) Search\n\tc) Sort\n\tq) Quit\n\nChoose action: ";

                string actionCode = Input.UserInput(cardsText + text, ["s", "c", "q"]);
                switch (actionCode)
                {
                    case SEARCH_PHRASE:
                        App.ClearScreen();

                        App.Write("Enter a text to search by: ", nextLine: false, space: false);
                        string searchOption = App.ReadLine();

                        List<DeckEntry> entries = [..deck.SearchByText(searchOption, out _)];

                        text = "\nPress any key to continue...";
                        cardsText = ComposeEntryList(entries);
                        
                        App.Write(cardsText + text, nextLine: true);

                        App.ReadKey();
                        break;
                    
                    case SORT_PHRASE: 
                        throw new NotImplementedException();

                    case QUIT_PHRASE: quit = "q"; break;

                    default:
                        throw new InvalidOperationException("Invalid input!");
                }

                if (quit.Equals("q")) break;
            }
        }
    }

    private static async Task ReviewMode()
    {
        while (true)
        {
            await UpdateDeckInfo();

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

            int deckIndex = Input.UserInput(text, DeckProvider.DeckPaths.Length, out string quit, keyPhrases: [QUIT_PHRASE]);

            if (quit.Equals(QUIT_PHRASE)) break;

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
                App.Write("All cards reviewed!\nPress any button to return...", nextLine: true);
                App.ReadKey();
            }
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
        App.Write(problemText);

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
            App.Write(text);

            string userInput = App.ReadLine();

            if (userInput.Equals("q"))
            {
                wantToEnd = true;

                return;
            }
            else if (userInput.Equals("s"))
                return;

            App.Write("Are you sure ('enter' to agree)? ", space: false);

            if (App.ReadLine().Length <= 0)
            {
                char separator = ConfigProvider.AppConfig.SeparatorSymbol;
                string[] answerParts = userInput.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                for (int i = 0; i < answerParts.Length; i++)
                {
                    if (int.TryParse(answerParts[i], out int index))
                        if (index - 1 < answeroptions.Length)
                            answerParts[i] = answeroptions[index - 1];
                    else
                        answerParts[i] = "UNRECOGNIZED!";
                }

                string answer = string.Join(separator, answerParts);
                bool isCorrect = entry.Answer(answer);

                App.Write("\n" + (isCorrect ? "Correct!" : "Mistake!"), nextLine: true, space: false);
                App.ReadLine();

                break;
            }
        }
    }

    private static async Task UpdateDeckInfo()
    {
        if (!ConfigProvider.AppConfig.LiteMode)
        {
            await DeckProvider.UpdateDecks();

            entryCounts = DeckProvider.GetEntryCounts().ToList();

            overallReviewedCount = 0;
            foreach (var deck in DeckProvider.Decks)
                overallReviewedCount += deck.ReviewCount;
        }
    }
}