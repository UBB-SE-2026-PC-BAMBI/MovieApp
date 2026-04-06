namespace MovieApp.Core.Models;

public sealed class JoinEventResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}