using Microsoft.Data.Sqlite;

namespace ProjectLauncher.Data.EF;

public static class ProjectLauncherDatabase
{
    public const string DataDirectoryEnvironmentVariable = "PROJECT_LAUNCHER_DATA_DIRECTORY";

    public static string GetDataDirectory()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable(DataDirectoryEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            return Path.GetFullPath(configuredDirectory);
        }

        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(localApplicationData, "ProjectLauncher");
    }

    public static string GetDatabasePath() =>
        Path.Combine(GetDataDirectory(), "project-launcher.db");

    public static string CreateConnectionString()
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = GetDatabasePath(),
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
        }.ToString();
    }
}

