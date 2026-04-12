using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using NRT.Flow;
using System.Linq;

namespace NRT.Resource;

public class Deck
{
    internal const string FILE_EXTENSION = "json";

    internal static readonly char[] InvalidCharsInPathName = Path.GetInvalidFileNameChars(); 
    internal string FilePath => Path.Combine(App.EntriesPath, $"{DeckTitle}.{FILE_EXTENSION}");

    public string DeckTitle { get; init; } = string.Empty;
    public List<DeckEntry> Entries { get; init; } = [];
    public bool UseSuperMemo { get; init; } = false;

    public DateTime? LastReviewDate { get; set; } = null;
    public int ReviewCount { get; set; } = 0;

    public Deck() { }

    public Deck(string title, bool useSuperMemo)
    {
        if (title.IndexOfAny(App.NOT_ALLOWED_CHARS) != -1)
            throw new InvalidDataException("Title contains not allowed symbols!");
        
        DeckTitle = title;
        UseSuperMemo = useSuperMemo;
    }

    public static string GetDeckNameFromPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return Path.GetFileNameWithoutExtension(path);
    }

    public static Result<string[]> GetPaths(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        List<string> paths = [];
        string[] allPaths = Directory.GetFiles(directoryPath);
        
        foreach (string path in allPaths)
            if (path.EndsWith($".{FILE_EXTENSION}"))
                paths.Add(path);

        if (paths.Count <= 0)
            return Result<string[]>.Fail(new FileNotFoundException($"There is no decks in \'{directoryPath}\'"));
        
        return Result<string[]>.Ok(paths.ToArray());
    } 

    public void OnReview(int reviewCount = 1) // when deck entry was answered
    {
        if (LastReviewDate == null)
            LastReviewDate = DateTime.Now.Date;
        else if (LastReviewDate.Value != DateTime.Now.Date)
        {
            LastReviewDate = DateTime.Now.Date;
            ReviewCount = 0;
        }

        ReviewCount += reviewCount;
    }

    public void Delete() => File.Delete(FilePath);

    public List<DeckEntry> SearchByText(string search, out List<int> indices)
    {
        List<DeckEntry> entries = [];
        indices = [];

        for (int i = 0; i < Entries.Count; i++)
        {
            var entry = Entries[i];

            bool contains = entry.Title.Contains(search);
            contains |= entry.Question.Contains(search);
            contains |= entry.GetAnswerOptions(false).Contains(search);
            contains |= entry.GetCorrectAnswer(false).Contains(search);

            if (contains)
            {
                entries.Add(entry);
                indices.Add(i);
            }
        }

        return entries;
    }

    public IEnumerable<DeckEntry> GetEntriesByDate(DateTime date) =>
        Entries.Where(e => e.ShowDate == null || e.ShowDate.Value.Date <= date.Date);
    
    public async Task AddEntry(DeckEntry entry) 
    {
        Entries.Add(entry);
        entry.ConnectToDeck(this);

        await WriteDeckAsync(this);
    }

    public static async Task<Result<Deck>> ReadDeckAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return await IO.TryDeserializeAsync<Deck>(path);
    }

    public static async Task<Result<bool>> WriteDeckAsync(Deck deck)
    {
        ArgumentNullException.ThrowIfNull(deck);

        string path = deck.FilePath;

        return await IO.TrySerializeAsync(deck, path);
    }

    public static async Task<Result<Deck>> ModifyDeckAsync(Deck deckToModify, Func<Deck, Deck> modify)
    {
        Deck modified = modify(deckToModify);
        Result<bool> result = await WriteDeckAsync(modified);

        if (result.Success)
        {
            if (!deckToModify.DeckTitle.Equals(modified.DeckTitle))
                File.Delete(deckToModify.FilePath);

            if (modified.UseSuperMemo)
            {
                foreach (var entry in modified.Entries)
                    entry.AnswersOptions = [];
            }

            return Result<Deck>.Ok(modified);
        }        
        else
            return Result<Deck>.Fail(result.Exception);
    }
    
    public async Task Setup()
    {
        foreach (var entry in Entries)
            entry.ConnectToDeck(this);

        OnReview(0); // Update review count

        await WriteDeckAsync(this);
    }
}