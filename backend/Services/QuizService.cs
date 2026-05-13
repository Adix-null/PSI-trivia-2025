using backend.Data;
using backend.Models;
using backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class QuizService
{
    private readonly AppDbContext _db;

    public QuizService(AppDbContext db)
    {
        _db = db;
    }

    private void ValidateQuiz(Quiz quiz)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(quiz.Title))
        {
            throw new QuizValidationException(
                "Quiz title cannot be empty or whitespace.",
                nameof(quiz.Title),
                quiz.Title
            );
        }

        if (quiz.Title.Length > 100)
        {
            throw new QuizValidationException(
                "Quiz title cannot exceed 100 characters.",
                nameof(quiz.Title),
                quiz.Title.Length
            );
        }

        // Validate questions (allow empty quizzes for initial creation)
        if (quiz.Questions != null && quiz.Questions.Count > 0)
        {
            if (quiz.Questions.Count > 50)
            {
                throw new QuizValidationException(
                    "Quiz cannot have more than 50 questions.",
                    nameof(quiz.Questions),
                    quiz.Questions.Count
                );
            }

            // Validate each question
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var question = quiz.Questions[i];

                if (string.IsNullOrWhiteSpace(question.QuestionText))
                {
                    throw new QuizValidationException(
                        $"Question #{i + 1}: Question text cannot be empty.",
                        $"Questions[{i}].QuestionText",
                        question.QuestionText
                    );
                }

                if (question.Options == null || question.Options.Count < 2)
                {
                    throw new QuizValidationException(
                        $"Question #{i + 1}: Must have at least 2 options.",
                        $"Questions[{i}].Options",
                        question.Options?.Count ?? 0
                    );
                }

                if (question.Options.Count > 6)
                {
                    throw new QuizValidationException(
                        $"Question #{i + 1}: Cannot have more than 6 options.",
                        $"Questions[{i}].Options",
                        question.Options.Count
                    );
                }

                if (question.CorrectOptionIndex < 0 || question.CorrectOptionIndex >= question.Options.Count)
                {
                    throw new QuizValidationException(
                        $"Question #{i + 1}: Correct option index {question.CorrectOptionIndex} is out of range (0-{question.Options.Count - 1}).",
                        $"Questions[{i}].CorrectOptionIndex",
                        question.CorrectOptionIndex
                    );
                }

                if (question.TimeLimit < 5 || question.TimeLimit > 120)
                {
                    throw new QuizValidationException(
                        $"Question #{i + 1}: Time limit must be between 5 and 120 seconds.",
                        $"Questions[{i}].TimeLimit",
                        question.TimeLimit
                    );
                }

                // Check for empty options
                for (int j = 0; j < question.Options.Count; j++)
                {
                    if (string.IsNullOrWhiteSpace(question.Options[j].OptionText))
                    {
                        throw new QuizValidationException(
                            $"Question #{i + 1}: Option #{j + 1} cannot be empty.",
                            $"Questions[{i}].Options[{j}]",
                            question.Options[j].OptionText
                        );
                    }
                }
            }
        }
    }

    // Get all quizzes
    public async Task<List<Quiz>> GetAllQuizzesAsync()
    {
        return await _db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .AsNoTracking()
            .OrderBy(q => q.ID)
            .ToListAsync();
    }

    public async Task<List<Quiz>> GetQuizzesByUserIdAsync(string userId)
    {
        if (!int.TryParse(userId, out var creatorId))
        {
            return new List<Quiz>();
        }

        return await _db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .Where(q => q.CreatorID == creatorId)
            .AsNoTracking()
            .OrderBy(q => q.ID)
            .ToListAsync();
    }

    public async Task<Quiz?> GetQuizByIdAsync(int quizId)
    {
        return await _db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.ID == quizId);
    }

    public async Task<Quiz> CreateQuizAsync(Quiz quiz)
    {
        // Validate quiz before creating
        ValidateQuiz(quiz);

        await _db.Quizzes.AddAsync(quiz);
        await _db.SaveChangesAsync();
        return quiz;
    }

    public async Task<bool> DeleteQuizAsync(int quizId)
    {
        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.ID == quizId);
        if (quiz == null)
        {
            return false;
        }

        _db.Quizzes.Remove(quiz);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Quiz?> UpdateQuizAsync(int quizId, Quiz updatedQuiz)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.ID == quizId);

        if (quiz == null)
        {
            return null;
        }

        // Validate quiz before updating
        ValidateQuiz(updatedQuiz);

        quiz.Title = updatedQuiz.Title;
        quiz.Description = updatedQuiz.Description;

        var incomingIds = updatedQuiz.Questions
            .Where(q => q.Id > 0)
            .Select(q => q.Id)
            .ToHashSet();

        var toRemove = quiz.Questions
            .Where(q => !incomingIds.Contains(q.Id))
            .ToList();

        if (toRemove.Count > 0)
        {
            _db.QuizQuestions.RemoveRange(toRemove);
            foreach (var remove in toRemove)
            {
                quiz.Questions.Remove(remove);
            }
        }

        foreach (var question in updatedQuiz.Questions)
        {
            if (question.Id > 0)
            {
                var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == question.Id);
                if (existingQuestion != null)
                {
                    existingQuestion.QuestionText = question.QuestionText;
                    existingQuestion.CorrectOptionIndex = question.CorrectOptionIndex;
                    existingQuestion.TimeLimit = question.TimeLimit;

                    // Sync Options
                    var incomingOptionIds = question.Options
                        .Where(o => o.Id > 0)
                        .Select(o => o.Id)
                        .ToHashSet();

                    var optionsToRemove = existingQuestion.Options
                        .Where(o => !incomingOptionIds.Contains(o.Id))
                        .ToList();

                    foreach (var opt in optionsToRemove)
                    {
                        existingQuestion.Options.Remove(opt);
                    }

                    foreach (var option in question.Options)
                    {
                        if (option.Id > 0)
                        {
                            var existingOption = existingQuestion.Options.FirstOrDefault(o => o.Id == option.Id);
                            if (existingOption != null)
                            {
                                existingOption.OptionText = option.OptionText;
                            }
                        }
                        else
                        {
                            existingQuestion.Options.Add(new Option
                            {
                                OptionText = option.OptionText
                            });
                        }
                    }
                    continue;
                }
            }

            quiz.Questions.Add(new QuizQuestion
            {
                QuestionText = question.QuestionText,
                Options = question.Options.Select(o => new Option
                {
                    OptionText = o.OptionText
                }).ToList(),
                CorrectOptionIndex = question.CorrectOptionIndex,
                TimeLimit = question.TimeLimit
            });
        }

        await _db.SaveChangesAsync();
        return quiz;
    }

    public async Task<bool> IncrementQuizPlaysAsync(int quizId)
    {
        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.ID == quizId);
        if (quiz == null)
        {
            return false;
        }

        if (quiz.TimesPlayed < 0)
        {
            quiz.TimesPlayed = 0;
        }

        quiz.TimesPlayed += 1;
        await _db.SaveChangesAsync();
        return true;
    }

    // Get top quizzes by times played
    public async Task<List<(int QuizId, string Title, string CreatorUsername, int TimesPlayed)>> GetTopQuizzesAsync(int limit = 10)
    {
        return await _db.Quizzes
            .Include(q => q.Creator)
            .OrderByDescending(q => q.TimesPlayed)
            .Take(limit)
            .Select(q => new ValueTuple<int, string, string, int>(
                q.ID,
                q.Title,
                q.Creator!.Username,
                q.TimesPlayed
            ))
            .ToListAsync();
    }
}