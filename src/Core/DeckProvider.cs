using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using NRT.Flow;
using NRT.Resource;

namespace NRT.Core;

public static class DeckProvider
{
    public static string[] DeckPaths { get; private set; } = [];
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

    public static IEnumerable<string> GetDeckNames(bool toLower = false)
    {
        if (DeckPaths.Length <= 0)
            throw new InvalidOperationException("DeckPath list does not contain anything! Run 'DeckProvider.LoadDeckPaths()' first!");

        foreach (var path in DeckPaths) 
        {
            string name = Deck.GetDeckNameFromPath(path);
            
            yield return toLower ? name.ToLower() : name;
        }
    }

    public static IEnumerable<int> GetEntryCounts()
    {
        if (Decks.Count <= 0)
            throw new InvalidOperationException("Deck list does not contain anything! Run 'DeckProvider.LoadDecks()' first!");

        foreach (var deck in Decks)
            yield return deck.GetEntriesByDate(DateTime.Now).Count();
    }

    public static async Task DeleteDeck(int index, bool localy = true)
    {
        if (DeckPaths.Length <= 0)
            throw new InvalidOperationException("DeckPath list does not contain anything! Run 'DeckProvider.LoadDeckPaths()' first!");

        if (localy && Decks.Count <= 0)
            throw new InvalidOperationException("Deck list does not contain anything! Run 'DeckProvider.LoadDecks()' first!");

        if (localy && index >= Decks.Count || index < 0)
            throw new InvalidDataException($"Decks list does not contain index {index}!");

        Deck deckToDelete = await ProvideDeck(index, DeckPaths[index], true);
        deckToDelete.Delete();

        if (localy)
            Decks.RemoveAt(index);

        LoadDeckPaths();
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

    public static async Task<Deck> ProvideDeck(int index, string path, bool readFromFile = false)
    {
        if (readFromFile)
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