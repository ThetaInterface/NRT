using System;
using System.Threading.Tasks;

using NRT.Flow;

namespace NRT.Interface;

public static class MainMenu
{
    public static async Task Show()
    {
        while (true)
        {
            App.ClearScreen();

            char action = Input.UserInput("\t1) Browse decks\n\t2) Deck master\n\t3) Settings\n\tq) Exit\n\nChoose action:",
                ['1', '2', '3', 'q']);

            switch (action)
            {
                case '1': await DeckBrowser.Show(); break;
                case '2': await DeckMaster.Show(); break;
                case '3': throw new NotImplementedException();
                
                case 'q': return;
            }
        }
    }
}