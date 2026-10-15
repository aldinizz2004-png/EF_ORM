using FootballTrainingManagement.Entities;
using FootballTrainingManagement.Services;
using Spectre.Console;
using static FootballTrainingManagement.UI.ConsoleInput;

namespace FootballTrainingManagement.UI;

// Console menus. All database work is delegated to the services.
public class Menus
{
    private readonly PlayerService _players;
    private readonly CoachService _coaches;
    private readonly TrainingSessionService _sessions;
    private readonly AttendanceService _attendance;

    public Menus(
        PlayerService players,
        CoachService coaches,
        TrainingSessionService sessions,
        AttendanceService attendance)
    {
        _players = players;
        _coaches = coaches;
        _sessions = sessions;
        _attendance = attendance;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ShowBanner();

            var choice = Menu("Exit",
                "Manage Players",
                "Manage Coaches",
                "Manage Training Sessions",
                "Assign Players to Training Sessions");

            switch (choice)
            {
                case 1: await PlayersMenuAsync(); break;
                case 2: await CoachesMenuAsync(); break;
                case 3: await SessionsMenuAsync(); break;
                case 4: await AttendanceMenuAsync(); break;
                case 0: return;
            }
        }
    }

    /* ==================== 1. Players ==================== */

    private async Task PlayersMenuAsync()
    {
        while (true)
        {
            ShowHeader("Manage Players");

            var choice = Menu("Back",
                "Add a new player",
                "Display all players",
                "Find a player by ID",
                "Update a player",
                "Delete a player");
            if (choice == 0) return;

            AnsiConsole.WriteLine();
            switch (choice)
            {
                case 1:
                {
                    var name = ReadRequiredString("Name:", 100);
                    var age = ReadInt("Age:", 5, 60);
                    var player = await _players.AddAsync(name, age);
                    Success($"Player added with ID {player.Id}.");
                    break;
                }
                case 2:
                    PrintPlayers(await _players.GetAllAsync(), "All Players");
                    break;
                case 3:
                {
                    var id = ReadId("player");
                    var player = await _players.GetByIdAsync(id);
                    if (player is null) Error($"Player with ID {id} was not found.");
                    else PrintPlayers(new List<Player> { player }, $"Player #{id}");
                    break;
                }
                case 4:
                {
                    var id = ReadId("player");
                    var player = await _players.GetByIdAsync(id);
                    if (player is null)
                    {
                        Error($"Player with ID {id} was not found.");
                        break;
                    }

                    Info($"Current: {player.Name}, age {player.Age}. Press Enter to keep a value.");
                    var name = ReadOptionalString("New name:", 100) ?? player.Name;
                    var age = ReadOptionalInt("New age:", 5, 60) ?? player.Age;
                    ShowResult(await _players.UpdateAsync(id, name, age));
                    break;
                }
                case 5:
                {
                    var id = ReadId("player");
                    if (Confirm($"Are you sure you want to delete player {id}?"))
                        ShowResult(await _players.DeleteAsync(id));
                    break;
                }
            }
            Pause();
        }
    }

    /* ==================== 2. Coaches ==================== */

    private async Task CoachesMenuAsync()
    {
        while (true)
        {
            ShowHeader("Manage Coaches");

            var choice = Menu("Back",
                "Add a new coach",
                "Display all coaches",
                "Find a coach by ID",
                "Update a coach",
                "Delete a coach");
            if (choice == 0) return;

            AnsiConsole.WriteLine();
            switch (choice)
            {
                case 1:
                {
                    var name = ReadRequiredString("Name:", 100);
                    var salary = ReadOptionalDecimal(
                        $"Monthly salary (press Enter for the standard {Coach.DefaultMonthlySalary:C}):",
                        CoachService.MinSalary, CoachService.MaxSalary);
                    ShowResult(await _coaches.AddAsync(name, salary));
                    break;
                }
                case 2:
                    PrintCoaches(await _coaches.GetAllAsync(), "All Coaches");
                    break;
                case 3:
                {
                    var id = ReadId("coach");
                    var coach = await _coaches.GetByIdAsync(id);
                    if (coach is null) Error($"Coach with ID {id} was not found.");
                    else PrintCoaches(new List<Coach> { coach }, $"Coach #{id}");
                    break;
                }
                case 4:
                {
                    var id = ReadId("coach");
                    var coach = await _coaches.GetByIdAsync(id);
                    if (coach is null)
                    {
                        Error($"Coach with ID {id} was not found.");
                        break;
                    }

                    Info($"Current: {coach.Name}, monthly salary {coach.Salary:C}. Press Enter to keep a value.");
                    var name = ReadOptionalString("New name:", 100) ?? coach.Name;
                    var salary = ReadOptionalDecimal("New monthly salary:",
                        CoachService.MinSalary, CoachService.MaxSalary) ?? coach.Salary;
                    ShowResult(await _coaches.UpdateAsync(id, name, salary));
                    break;
                }
                case 5:
                {
                    var id = ReadId("coach");
                    if (!await _coaches.ExistsAsync(id))
                    {
                        Error($"Coach with ID {id} was not found.");
                        break;
                    }

                    var sessionCount = await _coaches.CountSessionsAsync(id);
                    if (sessionCount > 0)
                        Info($"Warning: this coach has {sessionCount} training session(s). They will be deleted too.");

                    if (Confirm($"Are you sure you want to delete coach {id}?"))
                        ShowResult(await _coaches.DeleteAsync(id));
                    break;
                }
            }
            Pause();
        }
    }

    /* ==================== 3. Training Sessions ==================== */

    private async Task SessionsMenuAsync()
    {
        while (true)
        {
            ShowHeader("Manage Training Sessions");

            var choice = Menu("Back",
                "Create a new training session",
                "Display all training sessions",
                "Find a training session by ID",
                "Update a training session",
                "Delete a training session");
            if (choice == 0) return;

            AnsiConsole.WriteLine();
            switch (choice)
            {
                case 1:
                {
                    var title = ReadRequiredString("Title (must be unique):", 200);
                    var date = ReadDate($"Date ({DateFormat}):");
                    PrintCoaches(await _coaches.GetAllAsync(), "Available Coaches");
                    var coachId = ReadId("coach");
                    ShowResult(await _sessions.AddAsync(title, date, coachId));
                    break;
                }
                case 2:
                    PrintSessions(await _sessions.GetAllAsync(), "All Training Sessions");
                    break;
                case 3:
                {
                    var id = ReadId("training session");
                    var session = await _sessions.GetByIdAsync(id);
                    if (session is null) Error($"Training session with ID {id} was not found.");
                    else PrintSessionDetails(session);
                    break;
                }
                case 4:
                {
                    var id = ReadId("training session");
                    var session = await _sessions.GetByIdAsync(id);
                    if (session is null)
                    {
                        Error($"Training session with ID {id} was not found.");
                        break;
                    }

                    Info($"Current: '{session.Title}' on {session.Date.ToString(DateFormat)}, " +
                         $"coach {session.CoachId} ({session.Coach.Name}). Press Enter to keep a value.");
                    var title = ReadOptionalString("New title:", 200);
                    var date = ReadOptionalDate($"New date ({DateFormat}):");
                    var coachId = ReadOptionalInt("New coach ID:", 1);
                    ShowResult(await _sessions.UpdateAsync(id, title, date, coachId));
                    break;
                }
                case 5:
                {
                    var id = ReadId("training session");
                    if (Confirm($"Are you sure you want to delete training session {id}?"))
                        ShowResult(await _sessions.DeleteAsync(id));
                    break;
                }
            }
            Pause();
        }
    }

    /* ==================== 4. Assign Players ==================== */

    private async Task AttendanceMenuAsync()
    {
        while (true)
        {
            ShowHeader("Assign Players to Training Sessions");

            var choice = Menu("Back",
                "Assign a player to a training session",
                "Remove a player from a training session",
                "Display players in a training session",
                "Display training sessions attended by a player");
            if (choice == 0) return;

            AnsiConsole.WriteLine();
            switch (choice)
            {
                case 1:
                {
                    var sessionId = ReadId("training session");
                    var playerId = ReadId("player");
                    ShowResult(await _attendance.AssignPlayerAsync(sessionId, playerId));
                    break;
                }
                case 2:
                {
                    var sessionId = ReadId("training session");
                    var playerId = ReadId("player");
                    ShowResult(await _attendance.RemovePlayerAsync(sessionId, playerId));
                    break;
                }
                case 3:
                {
                    var sessionId = ReadId("training session");
                    var players = await _attendance.GetPlayersInSessionAsync(sessionId);
                    if (players is null) Error($"Training session with ID {sessionId} was not found.");
                    else PrintPlayers(players, $"Players in session #{sessionId}");
                    break;
                }
                case 4:
                {
                    var playerId = ReadId("player");
                    var sessions = await _attendance.GetSessionsForPlayerAsync(playerId);
                    if (sessions is null) Error($"Player with ID {playerId} was not found.");
                    else PrintSessions(sessions, $"Sessions attended by player #{playerId}");
                    break;
                }
            }
            Pause();
        }
    }

    /* ==================== Printing helpers ==================== */

    private static void PrintPlayers(List<Player> players, string title)
    {
        if (players.Count == 0)
        {
            Info("No players found.");
            return;
        }

        var table = CreateTable("ID", "Name", "Age");
        table.Title($"[bold green]{Markup.Escape(title)}[/]");
        table.Columns[0].RightAligned();
        table.Columns[2].Centered();

        foreach (var p in players)
            table.AddRow(p.Id.ToString(), Markup.Escape(p.Name), p.Age.ToString());

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]Total: {players.Count}[/]");
    }

    private static void PrintCoaches(List<Coach> coaches, string title)
    {
        if (coaches.Count == 0)
        {
            Info("No coaches found.");
            return;
        }

        var table = CreateTable("ID", "Name", "Monthly Salary", "Sessions");
        table.Title($"[bold green]{Markup.Escape(title)}[/]");
        table.Columns[0].RightAligned();
        table.Columns[2].RightAligned();
        table.Columns[3].Centered();

        foreach (var c in coaches)
            table.AddRow(
                c.Id.ToString(),
                Markup.Escape(c.Name),
                $"[green]{c.Salary:C}[/]",
                c.TrainingSessions.Count.ToString());

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]Total: {coaches.Count}[/]");
    }

    private static void PrintSessions(List<TrainingSession> sessions, string title)
    {
        if (sessions.Count == 0)
        {
            Info("No training sessions found.");
            return;
        }

        var table = CreateTable("ID", "Title", "Date", "Coach", "Players");
        table.Title($"[bold green]{Markup.Escape(title)}[/]");
        table.Columns[0].RightAligned();
        table.Columns[4].Centered();

        foreach (var t in sessions)
        {
            // Past sessions are shown in grey, upcoming ones in white
            var dateColor = t.Date < DateTime.Now ? "grey" : "white";
            table.AddRow(
                t.Id.ToString(),
                Markup.Escape(t.Title),
                $"[{dateColor}]{t.Date.ToString(DateFormat)}[/]",
                Markup.Escape($"{t.CoachId} - {t.Coach?.Name}"),
                t.Players.Count.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]Total: {sessions.Count}[/]");
    }

    // Detailed card for one session, including the names of its players
    private static void PrintSessionDetails(TrainingSession session)
    {
        var players = session.Players.Count == 0
            ? "[grey]No players assigned yet[/]"
            : string.Join("\n", session.Players
                .OrderBy(p => p.Name)
                .Select(p => $"• {Markup.Escape(p.Name)} [grey](ID {p.Id}, age {p.Age})[/]"));

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn()
            .AddRow("[yellow]Title[/]", Markup.Escape(session.Title))
            .AddRow("[yellow]Date[/]", session.Date.ToString(DateFormat))
            .AddRow("[yellow]Coach[/]", Markup.Escape($"{session.Coach.Name} (ID {session.CoachId})"))
            .AddRow("[yellow]Players[/]", players);

        AnsiConsole.Write(new Panel(grid)
            .Header($"[bold green] Training Session #{session.Id} [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green));
    }
}
