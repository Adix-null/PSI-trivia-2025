using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/moderation")]
public class ModerationController : ControllerBase
{
    private readonly ModerationService _moderationService;

    public ModerationController(ModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string? search)
    {
        if (!TryGetUserId(out _))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var users = await _moderationService.SearchUsersAsync(search);
        return Ok(users);
    }

    [HttpGet("users/{userId:int}/quizzes")]
    public async Task<IActionResult> GetUserQuizzes(int userId)
    {
        if (!TryGetUserId(out _))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var quizzes = await _moderationService.GetUserQuizzesAsync(userId);
        if (quizzes == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(quizzes);
    }

    [HttpGet("quizzes/{quizId:int}")]
    public async Task<IActionResult> GetQuizDetails(int quizId)
    {
        if (!TryGetUserId(out _))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var quiz = await _moderationService.GetQuizDetailsAsync(quizId);
        if (quiz == null)
        {
            return NotFound(new { message = "Quiz not found" });
        }

        return Ok(quiz);
    }

    [HttpPost("warn")]
    public async Task<IActionResult> Warn([FromBody] WarningRequest request)
    {
        if (!TryGetUserId(out var adminId))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var result = await _moderationService.WarnUserAsync(adminId, request.UserId, request.Message);
        if (result.UserNotFound)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!result.Success)
        {
            if (result.Error == "Unauthorized")
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            return BadRequest(new { message = result.Error });
        }

        return Ok(new { message = "Warning sent successfully" });
    }

    [HttpPost("ban")]
    public async Task<IActionResult> Ban([FromBody] BanRequest request)
    {
        if (!TryGetUserId(out var adminId))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var result = await _moderationService.BanUserAsync(adminId, request.UserId, request.Reason);
        if (result.UserNotFound)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!result.Success)
        {
            if (result.Error == "Unauthorized")
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            return BadRequest(new { message = result.Error });
        }

        return Ok(new { message = "User banned successfully" });
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out userId);
    }
}