using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NRT.Core;

namespace NRT.Resource;

public class DeckEntry
{
    internal const int HOURS_IN_ONE_DAY = 24;

    private Deck? parentLink;

    public string Title { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;

    public string[] AnswersOptions { get; set; } = [];
    public string[] CorrectAnswers { get; set; } = [];

    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? LastShownDate { get; set; } = null;
    public DateTime? ShowDate { get; set; } = null;

    public int CorrectAnswerStreak { get; set; } = 0;
    public double Difficulty { get; set; } = 2.5;

    public bool IsAnswersOrderImportant { get; set; } = false;
    
    public DeckEntry() { }

    public DeckEntry(string title, string question, string[] answers, string[] correctAnswers, bool isAnswerOrderImportant)
    {
        Title = title;
        Question = question;
        AnswersOptions = answers;
        CorrectAnswers = correctAnswers;
        IsAnswersOrderImportant = isAnswerOrderImportant;
    }

    public void ConnectToDeck(Deck deck) => parentLink = deck;

    public string GetCorrectAnswer(bool space)
    {
        string spaceStr = space ? " " : string.Empty;

        return string.Join(ConfigProvider.AppConfig.SeparatorSymbols.First() + spaceStr, CorrectAnswers);
    }

    public string GetAnswerOptions(bool space) 
    {
        string spaceStr = space ? " " : string.Empty;

        return string.Join(ConfigProvider.AppConfig.SeparatorSymbols.First() + spaceStr, AnswersOptions);
    }

    public void SetCorrectAnswer(string[] answer) => CorrectAnswers = answer;
    
    public void SetAnswerOptions(string[] options) => AnswersOptions = options;
    
    public bool Answer(string answer)
    {
        if (parentLink == null)
            throw new InvalidOperationException("Parent is not provided!");

        parentLink.OnReview();

        string[] answerParts = answer.Split(ConfigProvider.AppConfig.SeparatorSymbols, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        bool isCorrect = false;
        if (IsAnswersOrderImportant)
        {
            if (GetCorrectAnswer(false).Equals(string.Join(ConfigProvider.AppConfig.SeparatorSymbols.First(), answerParts)))
                isCorrect = true;
        }
        else
        {
            if (answerParts.Length != CorrectAnswers.Length)
                isCorrect = false;
            else
            {
                foreach (var part in answerParts)
                {
                    if (!CorrectAnswers.Contains(part))
                    {
                        isCorrect = false;

                        break;
                    }
                    else
                        isCorrect = true;
                }
            }
        }

        double intervalBetweenShowInDays = GetIntervalInDays(LastShownDate, ShowDate);

        LastShownDate = DateTime.Now.Date;
        ShowDate = LastShownDate;

        CorrectAnswerStreak = isCorrect ? CorrectAnswerStreak + 1 : 0;

        double willShowNextTimeAfterHours = CorrectAnswerStreak switch
        {
            0 => 0 * HOURS_IN_ONE_DAY,
            1 => 1 * HOURS_IN_ONE_DAY,
            2 => 3 * HOURS_IN_ONE_DAY,
            _ => intervalBetweenShowInDays * Difficulty * HOURS_IN_ONE_DAY
        };

        int willShowNextTimeAfterDays = (int)Math.Round(willShowNextTimeAfterHours / HOURS_IN_ONE_DAY, MidpointRounding.ToEven);
        willShowNextTimeAfterDays = Math.Min(willShowNextTimeAfterDays, 180);

        int quality = isCorrect ? 5 : 2;
        Difficulty = Math.Max(1.3f, Difficulty + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02)));
        Difficulty = Math.Round(Difficulty, 3);

        ShowDate = ShowDate.GetValueOrDefault().AddDays(willShowNextTimeAfterDays);

        return isCorrect;
    }

    public void Answer(int quality)
    {
        if (parentLink == null)
            throw new InvalidOperationException("Parent is not provided!");

        parentLink.OnReview();

        double intervalBetweenShowInDays = GetIntervalInDays(LastShownDate, ShowDate);

        LastShownDate = DateTime.Now.Date;

        if (quality < 3)
        {
            CorrectAnswerStreak = 0;
            Difficulty = Math.Max(1.3, Difficulty - 0.2);
            Difficulty = Math.Round(Difficulty, 3);
            ShowDate = LastShownDate;
        }
        else
        {
            CorrectAnswerStreak++;

            double willShowNextTimeAfterHours = CorrectAnswerStreak switch
            {
                1 => 1 * HOURS_IN_ONE_DAY,
                2 => 3 * HOURS_IN_ONE_DAY,
                _ => intervalBetweenShowInDays * Difficulty * HOURS_IN_ONE_DAY
            };

            int willShowNextTimeAfterDays = (int)Math.Round(willShowNextTimeAfterHours / HOURS_IN_ONE_DAY, MidpointRounding.ToEven);
            willShowNextTimeAfterDays = Math.Min(willShowNextTimeAfterDays, 180);

            Difficulty = Math.Max(1.3, Difficulty + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02)));
            Difficulty = Math.Round(Difficulty, 3);

            ShowDate = LastShownDate.GetValueOrDefault().AddDays(willShowNextTimeAfterDays);
        }
    }

    private static double GetIntervalInDays(DateTime? first, DateTime? second)
    {
        if (first == null || second == null)
            return 0;

        return (second - first).Value.TotalDays;
    }

    public string ToString(int? index = null)
    {
        if (parentLink == null)
            throw new InvalidOperationException("Parent is not provided!");
        
        string str = string.Empty;

        if (index != null)
            str = $"{index})";

        str += $"{(Title.Length <= 0 ? string.Empty : $"\tTitle:\t\t {Title}\n")}"
            + $"\tQuestion:\t {Question}\n";

        if (!parentLink.UseSuperMemo)
            str += $"\tAnswer options:  {GetAnswerOptions(true)}\n";
        
        str += $"\tCorrect answer:  {GetCorrectAnswer(true)}\n\n" +
            $"\tCreated:\t{CreationDate: yyyy.MM.dd}\n" +
            $"\tWill be shown:\t {(ShowDate != null ? ShowDate.GetValueOrDefault().ToString("yyyy.MM.dd") : "Today")}\n";

        return str;
    }
}