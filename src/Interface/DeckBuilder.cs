using System.Threading.Tasks;
using System.Collections.Generic;

using NRT.Flow;
using NRT.Resource;

namespace NRT.Interface;

public static class DeckBuilder
{
    private static string[] deckPaths = [];
    private static List<string> deckNames = [];

    public static void Show()
    {
        App.ClearScreen();

        Result<string[]> result = Deck.GetPaths(App.EntriesPath);

        if (result.Success)
        {
            deckPaths = result.Value;

            foreach (var path in deckPaths)
                deckNames.Add(Deck.GetDeckNameFromPath(path));
        }

        
    }
}