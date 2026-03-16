namespace NRT.Resource;

public struct Deck(string title)
{
    public string DeckTitle { get; set; } = title;
    public DeckEntry[] Entries { get; set; } = [];

    public void AddEntry(string title, string question, string[] answers, string correctAnswer) =>
        Entries = [..Entries, new(title, question, answers, correctAnswer)];
    
    public readonly string GetTitle() => DeckTitle;
}