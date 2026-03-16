using System;

namespace NRT.Resource;

public class DeckEntry(string title, string question, string[] answers, string correctAnswer)
{
    public string Title { get; set; } = title;
    public string Question { get; set; } = question;

    public string[] Answers { get; set; } = answers;
    public string CorrectAnswer { get; set; } = correctAnswer;

    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? ShownDate { get; set; } = null;
}