namespace backend.DTOs;

public record QuizQuestionPublicDto(int Id, string QuestionText, List<OptionDto> Options, int TimeLimit);

