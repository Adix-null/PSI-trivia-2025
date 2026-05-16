namespace backend.DTOs;

public class ModerationUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTimeOffset? BannedAt { get; set; }
}

public class WarningRequest
{
    public int UserId { get; set; }
    public required string Message { get; set; }
}

public class BanRequest
{
    public int UserId { get; set; }
    public required string Reason { get; set; }
}

public class ModerationQuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimesPlayed { get; set; }
    public int QuestionCount { get; set; }
}

public class ModerationQuizDetailsDto
{
    public int Id { get; set; }
    public int CreatorId { get; set; }
    public string CreatorUsername { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimesPlayed { get; set; }
    public List<ModerationQuestionDto> Questions { get; set; } = [];
}

public class ModerationQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int CorrectOptionIndex { get; set; }
    public int TimeLimit { get; set; }
    public List<ModerationOptionDto> Options { get; set; } = [];
}

public class ModerationOptionDto
{
    public int Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
}