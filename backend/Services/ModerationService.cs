using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ModerationService
{
    private readonly AppDbContext _db;

    public ModerationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ModerationUserDto>> SearchUsersAsync(string? search)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(normalizedSearch) ||
                u.Email.ToLower().Contains(normalizedSearch));
        }

        return await query
            .OrderBy(u => u.Username)
            .Take(50)
            .Select(u => new ModerationUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsBanned = u.IsBanned,
                BanReason = u.BanReason,
                BannedAt = u.BannedAt
            })
            .ToListAsync();
    }

    public async Task<List<ModerationQuizDto>?> GetUserQuizzesAsync(int userId)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return null;
        }

        return await _db.Quizzes
            .AsNoTracking()
            .Where(q => q.CreatorID == userId)
            .OrderBy(q => q.ID)
            .Select(q => new ModerationQuizDto
            {
                Id = q.ID,
                Title = q.Title,
                Description = q.Description,
                TimesPlayed = q.TimesPlayed,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync();
    }

    public async Task<ModerationQuizDetailsDto?> GetQuizDetailsAsync(int quizId)
    {
        return await _db.Quizzes
            .AsNoTracking()
            .Include(q => q.Creator)
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .Where(q => q.ID == quizId)
            .Select(q => new ModerationQuizDetailsDto
            {
                Id = q.ID,
                CreatorId = q.CreatorID,
                CreatorUsername = q.Creator != null ? q.Creator.Username : string.Empty,
                Title = q.Title,
                Description = q.Description,
                TimesPlayed = q.TimesPlayed,
                Questions = q.Questions
                    .OrderBy(question => question.Id)
                    .Select(question => new ModerationQuestionDto
                    {
                        Id = question.Id,
                        QuestionText = question.QuestionText,
                        CorrectOptionIndex = question.CorrectOptionIndex,
                        TimeLimit = question.TimeLimit,
                        Options = question.Options
                            .OrderBy(option => option.Id)
                            .Select(option => new ModerationOptionDto
                            {
                                Id = option.Id,
                                OptionText = option.OptionText
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string? Error, bool UserNotFound)> WarnUserAsync(int adminId, int userId, string message)
    {
        var adminError = await ValidateAdminAsync(adminId);
        if (adminError != null)
        {
            return (false, adminError, false);
        }

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return (false, null, true);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return (false, "Warning message is required", false);
        }

        if (message.Length > 1000)
        {
            return (false, "Warning message cannot exceed 1000 characters", false);
        }

        await _db.Warnings.AddAsync(new Warning
        {
            AdminId = adminId,
            UserId = userId,
            Message = message.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();
        return (true, null, false);
    }

    public async Task<(bool Success, string? Error, bool UserNotFound)> BanUserAsync(int adminId, int userId, string reason)
    {
        var adminError = await ValidateAdminAsync(adminId);
        if (adminError != null)
        {
            return (false, adminError, false);
        }

        if (adminId == userId)
        {
            return (false, "Admin cannot ban themselves", false);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return (false, null, true);
        }

        if (user.Role == "Admin")
        {
            return (false, "Cannot ban another admin", false);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Ban reason is required", false);
        }

        if (reason.Length > 1000)
        {
            return (false, "Ban reason cannot exceed 1000 characters", false);
        }

        user.IsBanned = true;
        user.BanReason = reason.Trim();
        user.BannedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null, false);
    }

    private async Task<string?> ValidateAdminAsync(int adminId)
    {
        var admin = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == adminId);
        if (admin == null || admin.Role != "Admin")
        {
            return "Unauthorized";
        }

        return null;
    }
}