# Project Launcher — Product and Technical Design Document

**Status:** Proposed V1  
**Project type:** Local-first desktop developer utility  
**Target stack:** C#, Avalonia UI, SQLite, EF Core, MVVM, CQRS-lite / vertical slices  
**Target platforms:** Windows 10/11 and Linux desktop  

---

## 1. Product Summary

Project Launcher is a local-first desktop application that helps a developer quickly understand the state of their local projects and resume work with minimal friction.

The user adds a project by selecting a folder. The application then:

- Uses the selected folder as the project
- Derives a default project name from the folder name
- Detects whether the folder belongs to a Git repository
- Reads the current branch, working-tree state, upstream state, remotes, and recent commit activity
- Converts a GitHub remote into a clickable repository link
- Lets the user record the next actions they intend to take
- Tracks commit streaks for each project and across all registered projects
- Launches the editor, terminal, folder, and GitHub page from one place

The product should answer four questions immediately:

1. What project should I work on?
2. What was I going to do next?
3. What state did I leave the repository in?
4. How quickly can I resume work?

---

## 2. Product Positioning

Project Launcher is not a generic task manager, Git client, or IDE replacement.

It is a **developer session launcher** that combines:

- Local project organization
- Read-only Git awareness
- Small project-specific action lists
- One-click workspace launching
- Lightweight development activity tracking

Its value comes from reducing the mental and mechanical overhead of returning to a project after hours, days, or weeks away.

### Portfolio value

The project demonstrates:

- C# desktop development
- Avalonia UI and MVVM
- SQLite and EF Core persistence
- Feature-first vertical slices
- Filesystem and process integration
- Git CLI integration and output parsing
- Async work and cancellation
- Cross-platform abstractions
- Derived application state
- Unit testing of deterministic business logic
- Local-first product design

---

## 3. Design Principles

- **Folder-first onboarding:** adding a project should require only a folder selection
- **Local-first:** no account, cloud database, or OAuth flow
- **Read Git, do not manage Git:** V1 inspects repositories but does not commit, push, pull, merge, or switch branches
- **Fast resume:** common launch actions should be available directly from each project card
- **Useful status, not noise:** show a small number of accurate, explainable signals
- **Manual intent and detected state are separate:** lifecycle describes the user's intent; badges describe repository facts
- **Small feature slices:** architecture should stay clean without becoming ceremonial
- **Finishability:** V1 must remain small enough to complete and polish

---

## 4. V1 Scope

### 4.1 Add a project from a folder

The main window includes an **Add Project** button.

Selecting it opens the operating system's folder picker. After the user chooses a folder, the app automatically creates the project without requiring a setup form.

The application should detect:

- Project name from the selected folder name
- Absolute local path
- Whether the folder is inside a Git working tree
- Git repository root, when available
- Current branch or detached HEAD state
- Origin remote URL
- Normalized GitHub web URL
- Upstream branch, when available
- Last commit date and summary
- Local Git author email and global Git author email

The selected folder remains the project's launch path. If it is inside a larger Git repository, Git commands use the detected repository root.

After creation, the user may optionally edit the display name, description, lifecycle, or repository URL.

### 4.2 Duplicate handling

The application prevents adding the same normalized local path twice.

If the user chooses a folder already registered, the app selects the existing project rather than creating a duplicate.

### 4.3 Non-Git folders

A selected folder does not have to be a Git repository.

Non-Git projects still support:

- Editor, terminal, and folder launching
- Next Actions
- Favorites
- Last-opened tracking

They display a **Not a Git Repository** badge and omit Git-specific details.

---

## 5. Main User Experience

V1 uses one primary window rather than a multi-page shell.

### 5.1 Main window layout

```text
┌─────────────────────────────────────────────────────────────────────┐
│ Project Launcher       Overall streak: 6 days        + Add Project │
├─────────────────────────────────────────────────────────────────────┤
│ Search projects...       All | Active | Dirty | Stale | Favorites │
├─────────────────────────────────────────────────────────────────────┤
│ ★ ResourceTool                                  ACTIVE              │
│   main · Dirty · Ahead 2 · Last commit today                        │
│   Project streak: 4 days                                           │
│                                                                     │
│   Next: Finish import validation error reporting                    │
│                                                                     │
│   [ Resume ] [ Editor ] [ Terminal ] [ Folder ] [ GitHub ] [ ••• ] │
├─────────────────────────────────────────────────────────────────────┤
│   Homelab Dashboard                              ACTIVE · STALE      │
│   main · Clean · Last commit 35 days ago                             │
│   Project streak: 0 days                                           │
│                                                                     │
│   Next: Decide the final V1 module scope                            │
│                                                                     │
│   [ Resume ] [ Editor ] [ Terminal ] [ Folder ] [ GitHub ] [ ••• ] │
└─────────────────────────────────────────────────────────────────────┘
```

### 5.2 Project card content

Each card should show:

- Favorite indicator
- Project name
- Manual lifecycle
- Current branch
- Important detected badges
- Last commit relative date
- Current project commit streak
- First incomplete Next Action
- Resume and quick-launch buttons

Secondary details should be available in an expandable details panel or project dialog rather than permanently crowding the card.

### 5.3 Search, filter, and sort

V1 supports:

- Search by project name or path
- Filter by lifecycle
- Filter by detected state: Dirty, Ahead, Behind, Stale, or Missing
- Favorites-only filter
- Sort by last opened, last commit, name, or recently added

The default sort should place favorites first, then most recently opened projects.

---

## 6. Launch Actions

Each project exposes the following actions:

### Resume

The primary action. It:

1. Opens the project in the configured editor
2. Optionally opens a terminal at the project path
3. Updates `LastOpenedAt`
4. Moves the project toward the top of the recently used list

Opening a terminal as part of Resume should be configurable.

### Open Editor

Opens the selected folder in the configured editor. VS Code is the default when the `code` command is available.

### Open Terminal

Opens the configured terminal with the working directory set to the project folder.

### Open Folder

Opens the project folder in the operating system's file manager.

### Open GitHub

Opens the normalized GitHub repository URL in the default browser. The button is disabled when no GitHub remote or manual URL exists.

### Copy Path

Copies the local project path to the clipboard. This is a small, inexpensive quality-of-life feature suitable for V1.

### Platform abstraction

Launch behavior should be behind a small interface such as `IWorkspaceLauncher` so Windows and Linux command differences do not leak into ViewModels.

The app should support configurable executable names and argument templates for the editor and terminal, including a `{path}` placeholder.

---

## 7. Next Actions

Each project has a small ordered list of **Next Actions**. This is intentionally narrower than a complete task-management system.

A Next Action contains:

- Short title
- Optional detail text
- Sort order
- Completion state
- Created date
- Completed date when applicable

### V1 behavior

- Add a Next Action from the project card or details panel
- Edit an action
- Mark an action complete
- Delete an action
- Move an action up or down
- Show the first incomplete action prominently on the project card
- Hide completed actions by default while retaining them in project history

### Scope boundary

V1 does not add priorities, due dates, reminders, recurring actions, dependencies, or cross-project boards. Those features would turn the launcher into a general task manager and weaken its focus.

---

## 8. Repository Lifecycle and Detected State

A single `Status` field is not sufficient because a repository can be Active, Dirty, Ahead, and Stale at the same time.

The design separates **manual lifecycle** from **detected repository state**.

### 8.1 Manual lifecycle

| Lifecycle | Meaning |
|---|---|
| Active | The user currently intends to work on or maintain the project |
| Paused | The project is intentionally inactive but still relevant |
| Archived | The project is retained for reference and hidden from normal active views |

New projects default to **Active**.

Archived is always manually selected. The application never archives a project automatically.

### 8.2 Detected state badges

| Badge | Detection rule |
|---|---|
| Clean | Git working tree has no staged, unstaged, or untracked changes |
| Dirty | Git working tree contains staged, unstaged, or untracked changes |
| Ahead N | Local `HEAD` is N commits ahead of its configured upstream |
| Behind N | Local `HEAD` is N commits behind its configured upstream |
| Diverged | Repository is both ahead and behind its upstream |
| Stale | Most recent local commit is older than the configured stale threshold |
| Detached | `HEAD` is detached rather than on a branch |
| No Upstream | Current branch has no configured upstream branch |
| No Remote | No Git remote is configured |
| Not Git | Selected folder is not inside a Git work tree |
| Missing | The registered local folder no longer exists |
| Unavailable | Git inspection failed for a recoverable reason |

The default stale threshold is **30 days** and can be changed in Configuration.

### 8.3 Important status rules

- `Active`, `Paused`, and `Archived` describe intent, not repository health
- `Stale` may coexist with `Active`
- `Dirty` may coexist with `Ahead`, `Behind`, or `Diverged`
- `Clean` and `Dirty` are mutually exclusive
- `Ahead` and `Behind` are based on local remote-tracking references unless the user explicitly refreshes from the remote
- V1 should not silently run network operations on startup

### 8.4 Optional read-only remote refresh

A **Fetch Status** action may run `git fetch --prune` and then recalculate ahead/behind counts. This is read-only with respect to the working tree but uses the network.

This should be a V1.1 feature unless implementation is trivial and the core V1 is already polished.

---

## 9. GitHub Linking

The application should link a project to GitHub without requiring the GitHub API.

### Detection process

1. Prefer the `origin` remote
2. If `origin` is absent, inspect other remotes for a GitHub URL
3. Normalize supported formats into an HTTPS browser URL

Examples:

```text
git@github.com:owner/repository.git
https://github.com/owner/repository.git
ssh://git@github.com/owner/repository.git
```

All normalize to:

```text
https://github.com/owner/repository
```

The user may override or manually enter a repository URL when detection fails.

### V1 boundary

V1 does not require:

- GitHub API tokens
- OAuth
- Stars, issues, pull requests, or workflow data
- Remote repository mutation

This keeps onboarding immediate and avoids unnecessary account integration.

---

## 10. Commit Streaks

Commit streaks provide a lightweight view of development consistency without requiring GitHub or cloud synchronization.

The application tracks:

- **Project current streak:** consecutive days with at least one matching commit in one project
- **Project longest streak:** longest historical sequence for that project
- **Overall current streak:** consecutive days with at least one matching commit in any registered project
- **Overall longest streak:** longest sequence across the union of activity dates from all registered projects
- **Commits today:** optional count across registered projects

### 10.1 What counts as a streak day

A calendar day counts once when at least one commit authored by the configured user identity exists on that date.

- Ten commits on one day still count as one streak day
- Commits to different projects on the same day still count as one overall streak day
- The same date may count independently toward several project streaks
- Merge commits count unless a later setting explicitly excludes them

### 10.2 Identifying the user's commits

On first use, the app reads:

- `git config --global user.email`
- Repository-local `git config user.email` values from added projects

These emails are added to a configurable **Commit Identities** list. Commit author email comparison is case-insensitive.

This supports users who commit with more than one email, such as a personal address and a GitHub no-reply address.

### 10.3 Date handling

The streak calculation uses each commit's authored timestamp and converts it to the user's local calendar date.

The current streak uses a morning-friendly grace rule:

- If the user has committed today, count backward starting today
- If the user has not committed today but committed yesterday, count backward starting yesterday
- If neither today nor yesterday has activity, the current streak is zero

This prevents a streak from appearing broken early in the day before the user has had a chance to commit.

### 10.4 Repository history scope

Project streaks should inspect commits reachable from all local refs so work on feature branches is included, not only commits currently reachable from `HEAD`.

The implementation should deduplicate commits by hash before deriving activity dates.

### 10.5 Display

The main header shows the overall current streak. Each project card shows its current project streak.

The details panel can show:

- Current streak
- Longest streak
- Last commit by the configured user
- Number of active commit days in the last 30 days

A contribution-style heatmap is useful, but belongs in V1.1 because it adds UI work without changing the core utility.

### 10.6 Streak limitations

The application can only see commits available in registered local repositories and local refs. It will not know about commits made only on another device until those commits exist locally.

This limitation should be stated clearly in the UI or README.

---

## 11. Git Inspection

Git inspection should be implemented through a small process runner and a dedicated `GitRepositoryInspector` service.

Representative read-only commands include:

```text
git -C <path> rev-parse --is-inside-work-tree
git -C <path> rev-parse --show-toplevel
git -C <path> branch --show-current
git -C <path> status --porcelain=v1 --branch
git -C <path> remote get-url origin
git -C <path> log -1 --all --format=%H|%cI|%s
git -C <path> rev-list --left-right --count HEAD...@{upstream}
git -C <path> log --all --format=%H|%aI|%ae
```

### Requirements

- Commands run asynchronously
- Commands have a timeout
- Cancellation is supported during refresh
- Standard output and error are captured separately
- Parsing is isolated from process execution
- A failure in one repository does not prevent other projects from loading
- ViewModels never parse raw Git output

### Refresh behavior

- Refresh all projects when the app opens
- Allow manual refresh of one project
- Allow manual refresh of all projects
- Do not run repeated polling in V1
- Display cached persisted project information immediately, then update detected state asynchronously

Git-derived state may remain in memory rather than being stored as canonical database data. Git is the source of truth.

---

## 12. Data Model

### Project

```text
Id
Name
Description?
Folder (ProjectFolder value object)
GitRootPath?
GitHubRepository?
Streak
Lifecycle
IsFavorite
LastOpenedAt?
CreatedAt
UpdatedAt
```

`Project` references an optional `GitHubRepository` entity and a required `ProjectStreak` entity. The project domain does not validate incoming values; validation occurs at the API/application boundary before a command changes persisted state.

### GitHubRepository

```text
Id
ProjectId
Owner
Name
WebUrl
OriginalRemoteUrl?
```

`GitHubRepository` is a domain entity, not a record embedded inside `Project`. It has a one-to-one relationship with a project and is optional because non-Git and non-GitHub projects remain valid.

### ProjectStreak

```text
Id
ProjectId
CurrentDays
LongestDays
LastCommitByUserAt?
ActiveCommitDaysLast30
CalculatedAt?
```

`ProjectStreak` is a domain entity with a required one-to-one relationship to a project. Its values are derived from Git commit activity and may be persisted as a cache for immediate display. Git history remains the source of truth, and the streak is recalculated during repository refresh.

### ProjectAction

```text
Id
ProjectId
Title
Details?
SortOrder
IsCompleted
CreatedAt
CompletedAt?
```

### GitIdentity

```text
Id
Email
IsEnabled
CreatedAt
```

### AppConfiguration

```text
Id
EditorExecutable
EditorArgumentsTemplate
TerminalExecutable
TerminalArgumentsTemplate
OpenTerminalOnResume
StaleAfterDays
```

### Runtime-only GitRepositorySnapshot

```text
ProjectId
IsGitRepository
RepositoryRoot?
CurrentBranch?
IsDetached
IsDirty
StagedFileCount
ModifiedFileCount
UntrackedFileCount
AheadCount?
BehindCount?
HasUpstream
RemoteUrl?
GitHubUrl?
LastCommitHash?
LastCommitSummary?
LastCommitAt?
LastRefreshAt
Error?
```

The snapshot is derived from Git and should not be treated as permanent domain truth.

---

## 13. Architecture

Use CQRS-lite to clearly separate UI, application/API behavior, domain entities, and EF Core persistence without adding a mediator or ceremonial application layers.

The solution uses four root projects:

```text
src/
  ProjectLaunch.Core.Domain/
    Project.cs
    GitHubRepository.cs
    ProjectStreak.cs
    ProjectAction.cs
    GitIdentity.cs
    AppConfiguration.cs

  ProjectLauncher.Core/
    Projects/
      Commands/
      Queries/
    GitHubRepositories/
      Commands/
      Queries/
    Streaks/
      Commands/
      Queries/
    ProjectActions/
      Commands/
      Queries/
    Configuration/
      Commands/
      Queries/
    Infrastructure/
      Git/

  ProjectLauncher.Data.EF/
    EntityConfigurations/
    Migrations/
    ApplicationDbContext.cs
    ApplicationUser.cs
    DependencyInjection.cs
    ImportLog.cs
    ProjectLauncher.Data.EF.csproj

  ProjectLauncher.UI.Avalonia/
    Assets/
    ViewModels/
    Views/
    App.axaml
    App.axaml.cs
    Program.cs
    ViewLocator.cs
    ProjectLauncher.UI.Avalonia.csproj
```

`ProjectLaunch.Core.Domain` defines the persistence-facing domain entities and relationships. Domain types contain state but do not validate input, access the filesystem, run Git, or depend on EF Core or Avalonia.

`ProjectLauncher.Core` is the application/API boundary. It owns validation, use-case orchestration, service interfaces, and CQRS-lite commands and queries. Commands and queries are grouped by the domain entity they modify or read. For example, `SetProjectNameCommand` belongs under `Projects/Commands`, while `GetProjectStreakQuery` belongs under `Streaks/Queries`.

`ProjectLauncher.Data.EF` contains EF Core persistence only: entity configurations, migrations, `ApplicationDbContext`, dependency-injection registration, and persistence support records. `ApplicationUser` represents at most one local profile in V1; it must not introduce authentication or cloud accounts. `ImportLog` is reserved for a future import workflow and should remain unused until that feature is promoted from V1.1.

`ProjectLauncher.UI.Avalonia` contains every UI-facing file, including Avalonia views, view models, assets, application builder/startup files, converters, controls, and platform-specific UI services. The UI calls the Core command/query boundary rather than accessing EF Core entities or `ApplicationDbContext` directly.

### Architecture rules

- Validation occurs in `ProjectLauncher.Core` command/query handlers, not in domain entities or Avalonia views
- Commands and queries are separated by the domain entity they modify or query
- Git process execution belongs in `ProjectLauncher.Core/Infrastructure/Git`
- EF Core configuration and migrations belong only in `ProjectLauncher.Data.EF`
- All Avalonia and UI-facing files belong in `ProjectLauncher.UI.Avalonia`
- Dependencies point inward: UI and Data.EF may depend on Core/Domain; Domain depends on neither
- Streak calculation remains deterministic and independent from Avalonia
- ViewModels coordinate use cases but do not contain Git parsing or database access
- CQRS-lite means separate command/query classes where useful, not a mediator requirement
- Avoid generic repositories unless EF Core access is genuinely duplicated
- Avoid domain events, plugins, event sourcing, and multiple application layers

---

## 14. Recommended V1 Features

The following extra features add real utility without meaningfully changing the product:

### Favorites

Pin important projects to the top of the list.

### Last opened tracking

Update `LastOpenedAt` whenever Resume, Editor, or Terminal is used. This helps sort projects by recent work.

### Missing-folder detection

Clearly identify projects that were moved, renamed, or deleted. Allow the user to locate the folder again or remove the project.

### Configurable stale threshold

Different users have different ideas of stale. Default to 30 days and expose one simple numeric setting.

### Copy path

A low-cost action that is frequently useful during development.

### Lightweight project notes

Allow one optional description field. Do not build a rich notes editor or runbook system in V1.

---

## 15. V1.1 Candidates

Build these only after the V1 success criteria are met:

- Scan a parent directory and suggest discovered Git repositories
- Contribution-style commit activity heatmap
- Read-only `git fetch` for accurate remote comparison
- Custom launch profiles per project
- Display last several commits
- Detect common solution/project files and show their technology
- Import/export local configuration
- Keyboard-first command palette
- System tray quick launcher

---

## 16. Explicitly Out of Scope for V1

- Git commit, push, pull, merge, rebase, reset, or checkout actions
- Embedded terminal or code editor
- GitHub API integration
- OAuth or user accounts
- GitHub issue and pull-request management
- Cloud synchronization
- Notifications and reminders
- Full task-management features
- Repository analytics dashboards
- Plugin architecture
- Automatic background polling
- Docker, server, or homelab monitoring
- AI-generated action suggestions

---

## 17. Error and Edge-Case Behavior

### Folder is not accessible

The API/application layer validates folder accessibility before executing the add-project command. Show a clear validation message and do not create the project.

### Git is not installed

The project can still be added and launched. Show a non-blocking message that Git status and streaks are unavailable.

### Folder is not a repository

Create the project normally and show the Not Git badge.

### Folder is inside a repository

Keep the selected folder as the launch path while recording the detected repository root for Git inspection.

### Repository has no commits

Show `No commits yet`, with streak values of zero.

### Repository has no upstream

Show No Upstream rather than treating it as an error.

### Remote is not GitHub

Retain the remote URL for display if useful, but disable the GitHub button unless a valid browser URL can be derived or manually entered.

### Repository is very large

Streak history inspection must run asynchronously. A later optimization may cache activity dates, but V1 should only add caching if real performance requires it.

### Several Git identities exist

Combine enabled identity emails and deduplicate commits by hash before calculating streaks.

---

## 18. Testing Strategy

### Unit tests

Prioritize pure logic:

- GitHub SSH/HTTPS URL normalization
- Git status output parsing
- Ahead/behind parsing
- Lifecycle and detected badge composition
- Current and longest streak calculations
- Today/yesterday streak grace behavior
- Multiple commits on the same day
- Multiple repositories contributing to the overall streak
- Multiple enabled author emails
- Stale threshold calculation
- Path normalization and duplicate detection

### Integration tests

Use temporary Git repositories to verify:

- Clean and dirty repository detection
- Branch detection
- Local commits and streak extraction
- Ahead/behind detection with a temporary bare remote
- Missing upstream behavior
- Non-Git directory handling

### UI tests

Keep UI automation small. Verify the most important flows manually and, where practical, test ViewModels without rendering Avalonia controls.

---

## 19. V1 Acceptance Criteria

V1 is complete when:

- The app launches as a desktop window
- A user can add a project by selecting only a folder
- The project name and Git root are detected automatically
- A GitHub remote is normalized into a browser link when available
- Projects persist between application launches
- The user can Resume, open the editor, terminal, folder, and GitHub page
- The user can add, edit, complete, reorder, and delete Next Actions
- The first incomplete action appears on the project card
- The app detects clean/dirty state, branch, last commit, upstream presence, and ahead/behind counts
- The user can manually mark a project Active, Paused, or Archived
- Stale state is calculated from a configurable threshold
- Per-project current streaks are displayed
- An overall current streak across registered projects is displayed
- Multiple commit identities are supported
- Missing folders and Git errors do not crash the application
- A fresh clone can build and run using documented steps
- The README includes screenshots, feature scope, setup instructions, and known limitations

---

## 20. Suggested Build Order

### Milestone 1 — Useful launcher

1. Create Avalonia application shell
2. Add SQLite and EF Core
3. Implement folder picker and Add Project
4. Persist and display project cards
5. Implement Editor, Terminal, Folder, and Copy Path actions
6. Implement Resume and Last Opened

At this point, the application is already useful.

### Milestone 2 — Git-aware launcher

1. Add Git process runner
2. Detect repository root and branch
3. Detect clean/dirty state
4. Detect remotes and normalize GitHub URLs
5. Read last commit
6. Add manual lifecycle and stale detection
7. Add ahead/behind detection from local tracking refs

At this point, the project has a strong portfolio story.

### Milestone 3 — Context and consistency

1. Add ordered Next Actions
2. Detect Git identities
3. Calculate project streaks
4. Calculate overall streak
5. Add search, filters, favorites, and sorting

### Milestone 4 — Polish

1. Loading and empty states
2. Error handling
3. Keyboard navigation
4. Unit and integration tests
5. README and screenshots
6. Build instructions for Windows and Linux

---

## 21. Cut Line

If time becomes limited, preserve the product in this order:

### Must keep

- Folder-only Add Project flow
- Persistence
- Project cards
- Resume / Editor / Terminal / Folder actions
- Next Action display
- Branch and clean/dirty detection
- GitHub link detection
- Project and overall current streaks

### Cut next

- Longest streak display
- Reordering actions
- Advanced filters
- Copy Path
- Multiple terminal templates
- Detailed change counts

### Defer completely

- Directory-wide repository discovery
- Heatmap
- Network fetch
- Custom launch profiles
- System tray
- GitHub API

---

## 22. README Product Description

> Project Launcher is a local-first desktop workspace launcher for developers managing multiple codebases. Add a folder once, then quickly resume work with one-click editor and terminal launching, local Git status, GitHub linking, project-specific next actions, and commit streak tracking—all without an account or cloud service.

---

## 23. Final Product Boundary

The finished V1 should remain easy to explain:

> Select a project folder. Project Launcher remembers it, tells you what state the repository is in, reminds you what to do next, tracks your commit consistency, and opens the tools you need to resume work.

That is the product. Everything else belongs after V1.
