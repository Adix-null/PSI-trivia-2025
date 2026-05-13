using System.Text.Json.Serialization;

namespace backend.Models;

public class Quiz
{
    public int ID { get; set; }

    public int CreatorID { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TimesPlayed { get; set; }

    [JsonIgnore]
    public User? Creator { get; set; }

    public List<QuizQuestion> Questions { get; set; } = [];

}

public class QuizQuestion
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public List<Option> Options { get; set; } = [];

    public int CorrectOptionIndex { get; set; }

    public int TimeLimit { get; set; }

    [JsonIgnore]
    public int QuizId { get; set; }

    [JsonIgnore]
    public Quiz? Quiz { get; set; }
}

public class Option
{
    public int Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int QuizQuestionId { get; set; }

    [JsonIgnore]
    public QuizQuestion? QuizQuestion { get; set; }
}

