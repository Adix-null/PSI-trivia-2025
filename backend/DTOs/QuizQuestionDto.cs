namespace backend.DTOs;

public record QuizQuestionDto(int Id, string QuestionText, List<OptionDto> Options, int CorrectOptionIndex, int TimeLimit);

