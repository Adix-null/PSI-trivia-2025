namespace backend.Models;

public class User : Interfaces.IJwtSubject
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsBanned { get; set; }

    public string? BanReason { get; set; }

    public DateTimeOffset? BannedAt { get; set; }

    public UserStats? Stats { get; set; }
}