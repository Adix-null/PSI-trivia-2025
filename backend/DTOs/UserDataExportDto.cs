namespace backend.DTOs;

public class UserDataExportDto
{
    public required DateTimeOffset ExportedAt { get; set; }
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public UserStatsDto? Stats { get; set; }
    public List<ExportedQuizDto> Quizzes { get; set; } = new();
}

public class ExportedQuizDto
{
    public int ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimesPlayed { get; set; }
    public List<ExportedQuestionDto> Questions { get; set; } = new();
}

public class ExportedQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }
    public int TimeLimit { get; set; }
}