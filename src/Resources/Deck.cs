using System.Collections.Generic;

namespace NRT.Resource;

public class Deck(string title)
{
    public string DeckTitle { get; set; } = title;
    public List<DeckEntry> Entries { get; set;}= [];

    public void AddEntry(string title, string question, string[] answers, string correctAnswer) =>
        Entries.Add(new(title, question, answers, correctAnswer));
}