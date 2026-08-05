namespace ProjectLauncher.Data.EF;

// Reserved for a future import workflow; import/export remains outside V1 scope.
public class ImportLog
{
    public int Id { get; set; }

    public DateTimeOffset ImportedAt { get; set; }

    public string Source { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public string? Error { get; set; }
}

