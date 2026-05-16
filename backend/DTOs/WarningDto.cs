namespace backend.DTOs;

public class WarningDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}