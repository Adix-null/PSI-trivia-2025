using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace backend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtService _jwtService;

    public UsersController(UserService userService, JwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    // Get all users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // Get user by ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var profile = await _userService.GetUserProfileAsync(userId);

        if (profile == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(profile);
    }

    [HttpGet("{id:int}/profile")]
    public async Task<IActionResult> GetUserProfile(int id)
    {
        var profile = await _userService.GetUserProfileAsync(id);

        if (profile == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var requesterId) || requesterId != id)
        {
            profile.Email = null;
            profile.BanReason = null;
            profile.BannedAt = null;
            profile.Warnings = [];
        }

        return Ok(profile);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string? query, [FromQuery] int limit = 5)
    {
        var results = await _userService.SearchUsersAsync(query, limit);
        return Ok(results);
    }

    // Delete user by ID
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteUserAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new { message = "User deleted successfully" });
    }

    // Update username
    [Authorize]
    [HttpPut("username")]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var (updatedUser, error) = await _userService.UpdateUsernameAsync(int.Parse(userId), request.Username);

        if (error != null)
        {
            return BadRequest(new { message = error });
        }

        var newToken = _jwtService.GenerateToken(updatedUser!);

        Response.Cookies.Append("authToken", newToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return Ok(new
        {
            message = "Username updated successfully",
            user = new
            {
                id = updatedUser!.Id,
                username = updatedUser.Username,
                email = updatedUser.Email
            },
            token = newToken
        });
    }

    // Update email
    [Authorize]
    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var (updatedUser, error) = await _userService.UpdateEmailAsync(int.Parse(userId), request.Email);

        if (error != null)
        {
            return BadRequest(new { message = error });
        }

        var newToken = _jwtService.GenerateToken(updatedUser!);

        Response.Cookies.Append("authToken", newToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return Ok(new
        {
            message = "Email updated successfully",
            user = new
            {
                id = updatedUser!.Id,
                username = updatedUser.Username,
                email = updatedUser.Email
            },
            token = newToken
        });
    }

    // Update password
    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var (success, error) = await _userService.UpdatePasswordAsync(
            int.Parse(userId),
            request.CurrentPassword,
            request.NewPassword
        );

        if (error != null)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Password updated successfully" });
    }
    
    // Export the current user's personal data
    [Authorize]
    [HttpGet("export")]
    public async Task<IActionResult> ExportMyData()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var data = await _userService.ExportUserDataAsync(userId);
        if (data == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        var fileName = $"trivia-data-export-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json";
        return File(bytes, "application/json", fileName);
    }

    // Delete the current user's account
    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        var deleted = await _userService.DeleteUserAsync(userId);
        if (!deleted)
        {
            return NotFound(new { message = "User not found" });
        }

        Response.Cookies.Delete("authToken");
        return Ok(new { message = "Account deleted successfully" });
    }
}