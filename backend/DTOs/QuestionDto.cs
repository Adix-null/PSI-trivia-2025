namespace backend.DTOs;

public record QuestionDto(int Index, string QuestionText, List<OptionDto> Options, DateTimeOffset EndsAt);

