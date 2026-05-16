using System.Text.Json.Serialization;

namespace backend.Models;

public class Warning
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AdminId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public User? User { get; set; }

    [JsonIgnore]
    public User? Admin { get; set; }
}