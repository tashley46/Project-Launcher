# Project Launcher CLI Reference

Run these commands from the repository root.

## First-time setup

Check that .NET 10 is installed:

```bash
dotnet --version
dotnet --info
```

Restore all .NET dependencies:

```bash
dotnet restore ProjectLauncher.slnx
```

Install optional Python requirements. On Linux distributions with an externally managed Python installation, create a virtual environment first:

```bash
python3 -m venv .venv
source .venv/bin/activate
python -m pip install -r requirements.txt
```

On Windows:

```powershell
py -m venv .venv
.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
```

Project Launcher currently has no Python package dependencies, so pip will not install any packages.

## Run the application

Run once:

```bash
dotnet run --project src/ProjectLauncher.UI.Avalonia
```

Run with automatic rebuild and restart when source files change:

```bash
dotnet watch --project src/ProjectLauncher.UI.Avalonia
```

## Build commands

Build the complete solution:

```bash
dotnet build ProjectLauncher.slnx
```

Build a release version:

```bash
dotnet build ProjectLauncher.slnx --configuration Release
```

Clean generated build output:

```bash
dotnet clean ProjectLauncher.slnx
```

## Entity Framework commands

Install the EF Core CLI tool if it is not already available:

```bash
dotnet tool install --global dotnet-ef
```

Check the installed EF CLI version:

```bash
dotnet ef --version
```

Create a migration:

```bash
dotnet ef migrations add MigrationName \
  --project src/ProjectLauncher.Data.EF \
  --startup-project src/ProjectLauncher.UI.Avalonia \
  --output-dir Migrations
```

Apply migrations to the configured database:

```bash
dotnet ef database update \
  --project src/ProjectLauncher.Data.EF \
  --startup-project src/ProjectLauncher.UI.Avalonia
```

List migrations:

```bash
dotnet ef migrations list \
  --project src/ProjectLauncher.Data.EF \
  --startup-project src/ProjectLauncher.UI.Avalonia
```

The application must configure `ApplicationDbContext` at startup before migration commands can create or update a database.

## NuGet package commands

List direct and transitive packages:

```bash
dotnet list ProjectLauncher.slnx package --include-transitive
```

Check for vulnerable packages:

```bash
dotnet list ProjectLauncher.slnx package --vulnerable --include-transitive
```

Add a package to a particular project:

```bash
dotnet add path/to/Project.csproj package Package.Name
```

Remove a package:

```bash
dotnet remove path/to/Project.csproj package Package.Name
```

## Useful pip commands

Show the active Python and pip versions:

```bash
python3 --version
python3 -m pip --version
```

Create and activate an optional virtual environment on Linux:

```bash
python3 -m venv .venv
source .venv/bin/activate
```

Create and activate an optional virtual environment on Windows PowerShell:

```powershell
py -m venv .venv
.venv\Scripts\Activate.ps1
```

Install the requirements file:

```bash
python3 -m pip install -r requirements.txt
```

On Windows, use `py -m pip install -r requirements.txt`.

List installed Python packages:

```bash
python3 -m pip list
```

Deactivate a virtual environment:

```bash
deactivate
```

Do not use `pip freeze` to populate `requirements.txt` unless Python tooling is intentionally added to the repository. The Avalonia application and EF projects use NuGet, not pip.
