namespace ProjectLauncher.Data.EF;

// Represents a local profile only; V1 does not include authentication or cloud accounts.
public class ApplicationUser
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}

