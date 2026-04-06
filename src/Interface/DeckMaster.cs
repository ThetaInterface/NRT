using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NRT.Core;
using NRT.Flow;
using NRT.Resource;

namespace NRT.Interface;

public static class DeckMaster
{
    internal const string QUIT_PHRASE = "q";
    internal const string SEARCH_PHRASE = "s";
    internal const string SORT_PHRASE = "c";
    internal const string CLEAR_PHRASE = "d";

    internal const string DEFAULT_KEY_PHRASE = "";

    public static async Task Show()
    {
        while (true)
        {
            App.ClearScreen();

            string textToShow = "\t1) Create new deck\n\t2) Edit existing deck\n\t3) Delete existing deck\n\tq) Quit\n\nChoose action: ";
            int userInput = Input.UserInput(textToShow, ceiling: 3, out string quit, keyPhrases: [QUIT_PHRASE]);

            if (quit.Equals(QUIT_PHRASE)) break;

            switch (userInput)
            {
                case 1: await CreateDeck(); break;
                case 2: await EditDeck(); break;
                case 3: await DeleteDeck(); break;

                default:
                    throw new InvalidOperationException("Invalid input!");
            }
        }
    }

    private static async Task CreateDeck(Deck? fromDeck = null)
    {
        string deckTitle;
        bool useSuperMemo;

        string textToShow;

        App.ClearScreen();

        Result<bool> result = DeckProvider.LoadDeckPaths();
        if (!result.Success && result.Exception is not FileNotFoundException)
            throw result.Exception;
        
        if (fromDeck == null)
            textToShow = "Enter a name for new deck ('q' to quit): ";
        else
            textToShow = "Enter a new name for deck ('q' to quit, 's' to skip): ";

        deckTitle = Input.UserInput(textToShow, [..App.NOT_ALLOWED_NAMES, ..DeckProvider.GetDeckNames(toLower: true)], App.NOT_ALLOWED_CHARS, inverted: true, toLower: false);

        if (deckTitle.Equals("q")) 
            return;
        else if (deckTitle.Equals("s") && fromDeck != null)
            deckTitle = fromDeck.DeckTitle;
        
        App.ClearScreen();
        
        if (fromDeck == null)
            textToShow = "Do you want create 'Test' deck? (y/n): ";
        else
            textToShow = $"Change deck type to {(fromDeck.UseSuperMemo ? "'Test' format" : "'Card' format")} (y/n): ";
        
        string userInput = Input.UserInput(textToShow, ["y", "n"]);

        if (userInput.Equals("y"))
            useSuperMemo = fromDeck != null && !fromDeck.UseSuperMemo;
        else
            useSuperMemo = fromDeck == null || fromDeck.UseSuperMemo;

        Result<Deck> modifyResult = await Deck.ModifyDeckAsync(fromDeck ?? new Deck(), newDeck => new Deck()
        {
            DeckTitle = deckTitle,
            Entries = fromDeck == null ? [] : fromDeck.Entries,
            UseSuperMemo = useSuperMemo,
            LastReviewDate = fromDeck?.LastReviewDate,
            ReviewCount = fromDeck == null ? default : fromDeck.ReviewCount
        });

        if (!modifyResult.Success)
            throw modifyResult.Exception;
        
        Deck newDeck = modifyResult.Value;
        Result<bool> writeResult = await Deck.WriteDeckAsync(newDeck);

        if (!writeResult.Success)
            throw writeResult.Exception;        
    }

    private static async Task EditDeck()
    {
        string textToShow;

        while (true) 
        {
            App.ClearScreen();

            Result<bool> result = DeckProvider.LoadDeckPaths();
            if (!result.Success && result.Exception is not FileNotFoundException)
                throw result.Exception;
            else if (!result.Success && result.Exception is FileNotFoundException)
            {
                App.Write("There's no created decks yet!", nextLine: true);
                App.ReadKey();

                return;
            }

            textToShow = DeckBrowser.GetNumberedDeckList();
            textToShow += "\nChoose deck to edit ('q' to quit): ";

            int deckIndex = Input.UserInput(textToShow, ceiling: DeckProvider.DeckPaths.Length, out string quit, keyPhrases: [QUIT_PHRASE]);

            if (quit.Equals(QUIT_PHRASE)) break;

            while (true) 
            {
                App.ClearScreen();

                Deck deckEdit = await DeckProvider.ProvideDeck(deckIndex - 1, DeckProvider.DeckPaths[deckIndex - 1], readFromFile: true);

                textToShow = "\t1) Change deck properties\n\t2) Add entry\n\t3) Edit entries\n\t4) Delete entries\n\tq) Quit\n\nChoose action: ";
                int userInput = Input.UserInput(textToShow, ceiling: 4, out quit, keyPhrases: [QUIT_PHRASE]);

                if (quit.Equals(QUIT_PHRASE)) break;

                switch (userInput)
                {
                    case 1: await CreateDeck(fromDeck: deckEdit); break;
                    case 2: await AddEntriesToDeck(toDeck: deckEdit); break;
                    case 3: await EditEntries(ofDeck: deckEdit, false); break;
                    case 4: await EditEntries(ofDeck: deckEdit, true); break;

                    default:
                        throw new InvalidOperationException("Invalid input!");
                }
            }
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
            App.Write("There's no created decks yet!", nextLine: true);
            App.ReadKey();

            return;
        }

        string textToShow = DeckBrowser.GetNumberedDeckList();
        textToShow += "\n\nChoose deck to delete ('q' to quit): ";

        int deckIndex = Input.UserInput(textToShow, ceiling: DeckProvider.DeckPaths.Length, out string quit, keyPhrases: [QUIT_PHRASE]);

        if (quit.Equals(QUIT_PHRASE)) return;

        App.Write("Are you sure? (press 'enter' to agree) ");
        string userInput = App.ReadLine();

        if (userInput.Equals(""))
            await DeckProvider.DeleteDeck(deckIndex - 1, false);
    }

    private static async Task AddEntriesToDeck(Deck toDeck)
    {
        string entryTitle;
        string entryQuestion;

        string[] entryAnswerOption = [];
        string[] entryCorrectAnswers;

        bool isAnswerOrderImportant = false;

        string prefix;
        string textToShow;
        string userInput;

        bool quit;

        while (true)
        {
            App.ClearScreen();

            App.Write("Enter a title for entry (press 'enter' to skip, enter 'q' to quit): ");
            entryTitle = Console.ReadLine() ?? throw new InvalidOperationException("Console input is null!");
            
            if (entryTitle.Equals("q")) return;

            prefix = "Title is set!\n";

            App.ClearScreen(); 

            textToShow = prefix + "Enter a question for entry ('q' to quit): ";
            entryQuestion = Input.UserInput(textToShow, App.NOT_ALLOWED_NAMES, inverted: true, toLower: false);

            if (entryQuestion.Equals("q")) return;

            prefix += "Question is set!\n";

            App.ClearScreen();

            if (!toDeck.UseSuperMemo)
            {
                textToShow = prefix + "Enter answer options for entry (press 'enter' to end): ";
                entryAnswerOption = Input.UserInput(textToShow, out quit, QUIT_PHRASE);

                if (entryAnswerOption.Length == 0 && quit) return;

                prefix += "Answer options is set!\n";
            }

            App.ClearScreen();
            
            textToShow = prefix + "Enter a correct answer for entry (press 'enter' to end): ";
            entryCorrectAnswers = Input.UserInput(textToShow, out quit, QUIT_PHRASE);

            if (entryCorrectAnswers.Length == 0 && quit) return;
        
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
            await toDeck.AddEntry(newEntry);

            App.ClearScreen();

            textToShow = "Create more? (y/n) ";
            userInput = Input.UserInput(textToShow, ["y", "n"]);

            if (userInput.Equals("n"))
                break;
        }
    }

    private static async Task EditEntries(Deck ofDeck, bool delete)
    {
        List<DeckEntry> entries = ofDeck.Entries;
        List<int> indices = [];

        string cardsText = DeckBrowser.ComposeEntryList(entries);
        string actionText = "\n\ts) Search\n\tc) Sort\n\td) Clear filters\n\tq) Quit\n\nChoose action or enter an index of card: ";

        while (true)
        {
            cardsText = DeckBrowser.ComposeEntryList(entries);

            int cardIndex = Input.UserInput(cardsText + actionText, ceiling: entries.Count, out string phrase, keyPhrases: [
                SEARCH_PHRASE, 
                SORT_PHRASE,
                CLEAR_PHRASE, 
                QUIT_PHRASE]);

            if (phrase.Equals(DEFAULT_KEY_PHRASE))
            {
                Deck modifiedDeck;
                Result<bool> writeResult;

                if (delete)
                {
                    List<DeckEntry> newEntries = [..ofDeck.Entries];

                    if (indices.Count == 0)
                        newEntries.RemoveAt(cardIndex - 1);
                    else
                        newEntries.RemoveAt(indices[cardIndex - 1]);

                    var result = await Deck.ModifyDeckAsync(ofDeck, modified => new Deck()
                    {
                        DeckTitle = ofDeck.DeckTitle,
                        Entries = newEntries,
                        UseSuperMemo = ofDeck.UseSuperMemo,
                        LastReviewDate = ofDeck.LastReviewDate,
                        ReviewCount = ofDeck.ReviewCount
                    });

                    if (!result.Success)
                        throw result.Exception;

                    modifiedDeck = result.Value;
                    writeResult = await Deck.WriteDeckAsync(modifiedDeck);

                    if (!writeResult.Success)
                        throw writeResult.Exception;     

                    return;
                }
                
                List<DeckEntry> newEntryList = [..ofDeck.Entries];
                DeckEntry newEntry = entries[cardIndex - 1];

                EditEntry(ref newEntry);

                if (indices.Count == 0)
                    newEntryList[cardIndex - 1] = newEntry;
                else
                    newEntryList[indices[cardIndex - 1]] = newEntry;

                var modifyResult = await Deck.ModifyDeckAsync(ofDeck, newDeck => new Deck()
                {
                    DeckTitle = ofDeck.DeckTitle,
                    Entries = newEntryList,
                    UseSuperMemo = ofDeck.UseSuperMemo,
                    LastReviewDate = ofDeck.LastReviewDate,
                    ReviewCount = ofDeck.ReviewCount
                });

                if (!modifyResult.Success)
                    throw modifyResult.Exception;

                modifiedDeck = modifyResult.Value;
                writeResult = await Deck.WriteDeckAsync(modifiedDeck);

                if (!writeResult.Success)
                    throw writeResult.Exception;
            }
            else
            {
                switch (phrase)
                {
                    case SEARCH_PHRASE:
                        
                        App.Write(cardsText + "\nEnter a text to search by: ", nextLine: false);
                        string searchOption = App.ReadLine();

                        entries = ofDeck.SearchByText(searchOption, out indices).ToList();

                        if (entries.Count != indices.Count)
                            throw new InvalidDataException("Entries count not equals indices' !");
                        break;

                    case SORT_PHRASE:
                        throw new NotImplementedException();
                    
                    case CLEAR_PHRASE:
                        entries = ofDeck.Entries;
                        indices = [];
                        break;

                    case QUIT_PHRASE:
                        return;
                }
            }
        }
    }

    private static void EditEntry(ref DeckEntry entry)
    {
        bool quit;

        while (true)
        {
            string entryText = entry.ToString();
            string actionText = "\n\t1) Edit title\n\t2) Edit question\n\t3) Edit answer options\n\t4) Edit correct answers\n\t5) Reset entry\n\tq) Quit" +
                "\n\nChoose action: ";

            int userInput = Input.UserInput(entryText + actionText, ceiling: 5, out string phrase, keyPhrases: [QUIT_PHRASE]);

            if (phrase.Equals("q")) break;

            switch (userInput)
            {
                case 1: 
                    App.ClearScreen();
                    App.Write($"Currect title: {entry.Title}\nEnter a new title ('q' to cancel): ", space: false);

                    string title = App.ReadLine();
                    if (title.ToLower().Equals(QUIT_PHRASE)) break;

                    entry.Title = title;
                    break;
                case 2:
                    App.ClearScreen();
                    actionText = $"Current question: {entry.Question}\nEnter a new question ('q' to cancel): ";

                    string question = Input.UserInput(actionText, ["", " "], inverted: true);
                    if (question.ToLower().Equals(QUIT_PHRASE)) break;

                    entry.Question = question; 
                    break;
                case 3: 
                    App.ClearScreen();

                    string[] answerOptions = Input.UserInput($"Current answer options: {entry.GetAnswerOptions()}\nEnter a new answer options ('q' to cancel): ",
                        out quit, QUIT_PHRASE);

                    if (answerOptions.Length ==0 && quit) break;

                    entry.SetAnswerOptions(answerOptions);
                    break;
                case 4: 
                    App.ClearScreen();

                    string[] correctAnswer = Input.UserInput($"Current correct answer: {entry.GetCorrectAnswer()}\nEnter a new correct answer ('q' to cancel): ",
                        out quit, QUIT_PHRASE);

                    if (correctAnswer.Length ==0 && quit) break;

                    entry.SetCorrectAnswer(correctAnswer);
                    break;
                case 5:
                    entry.LastShownDate = null;
                    entry.ShowDate = null;
                    entry.CorrectAnswerStreak = 0;
                    entry.Difficulty = 2.5; 
                    break;

                default: 
                    throw new InvalidOperationException("Invalid input!");
            }
        }
    }
}