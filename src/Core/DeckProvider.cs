using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using NRT.Flow;
using NRT.Resource;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace NRT.Core;

public static class DeckProvider
{
    [NotNull]
    public static string[] DeckPaths { get; private set; } = [];
    [NotNull]
    public static List<Deck> Decks { get; private set; } = [];

    public static Result<bool> LoadDeckPaths()
    {
        Result<string[]> result = Deck.GetPaths(App.EntriesPath);

        if (!result.Success)
        {
            Logger.Warning("Decks not found OR unable to read!", "DeckProvider");

            return Result<bool>.Fail(result.Exception);
        }

        DeckPaths = result.Value;

        return Result<bool>.Ok(true);
    }   

    public static IEnumerable<string> GetDeckNames()
    {
        if (DeckPaths.Length <= 0)
            throw new InvalidOperationException("DeckPath list does not contain anything! Run 'DeckProvider.LoadDeckPaths()' first!");

        foreach (var path in DeckPaths)
            yield return Deck.GetDeckNameFromPath(path);
    }

    public static IEnumerable<int> GetEntryCounts()
    {
        if (Decks.Count <= 0)
            throw new InvalidOperationException("DeckPath list does not contain anything! Run 'DeckProvider.LoadDeckPaths()' first!");

        foreach (var deck in Decks)
            yield return deck.GetEntriesByDate(DateTime.Now).Count();
    }

    public static async Task LoadDecks()
    {
        if (DeckPaths.Length <= 0)
            throw new InvalidOperationException("Deck paths does not contain anything! Run 'DeckProvider.LoadDeckPaths()' first!");
        
        Decks = [];
        foreach (var path in DeckPaths)
        {
            Result<Deck> result = await Deck.ReadDeckAsync(path);

            if (!result.Success)
            {
                Logger.Warning("Deck read error!", "DeckProvider");

                throw result.Exception;
            }

            Decks.Add(result.Value);
        }
    }

    public static async Task UpdateDecks()
    {
        if (Decks != null)
        {
            foreach (var deck in Decks)
                await deck.Setup();
        }
    }

    public static async Task<Deck> ProvideDeck(int index, string path, bool liteMode)
    {
        if (liteMode)
        {
            Result<Deck> result = await Deck.ReadDeckAsync(path);

            if (!result.Success)
            {
                Logger.Warning("Deck read error!", "DeckProvider");

                throw result.Exception;
            }

            Deck deck = result.Value;
            await deck.Setup();

            return deck;
        }
        else
            return Decks[index];
    }
}