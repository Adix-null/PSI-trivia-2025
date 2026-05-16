namespace backend.DTOs;

public class UserProfileDto
{
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTimeOffset? BannedAt { get; set; }
    public List<WarningDto> Warnings { get; set; } = [];
    public required UserStatsDto Stats { get; set; }
}

