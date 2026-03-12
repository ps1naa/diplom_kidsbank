namespace KidBank.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; internal set; }
    public DateTime Timestamp { get; internal set; }
    public string Level { get; internal set; } = string.Empty;
    public string Message { get; internal set; } = string.Empty;
    public string? Exception { get; internal set; }
    public string? RequestPath { get; internal set; }
    public string? RequestMethod { get; internal set; }
    public Guid? UserId { get; internal set; }
    public string? UserEmail { get; internal set; }
    public int? StatusCode { get; internal set; }
    public long? ElapsedMs { get; internal set; }
    public string? MachineName { get; internal set; }
}
