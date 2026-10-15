using System.Globalization;
using FootballTrainingManagement.Services;
using Spectre.Console;

namespace FootballTrainingManagement.UI;

// Helpers to draw the screens and read/validate user input.
// Spectre.Console is used only for the look (colors, tables, menus);
// all database work stays in the Services.
public static class ConsoleInput
{
    public const string DateFormat = "yyyy-MM-dd HH:mm";

    // Arrow-key menus only work in a real console window.
    // When input is piped (e.g. automated tests) we fall back to typing numbers.
    private static bool IsInteractive =>
        !Console.IsInputRedirected && !Console.IsOutputRedirected;

    /* ==================== Screens & menus ==================== */

    public static void ShowBanner()
    {
        ClearScreen();
        AnsiConsole.Write(new FigletText("Football").Centered().Color(Color.Green));
        AnsiConsole.Write(new Rule("[bold yellow]Training Management System[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();
    }

    public static void ShowHeader(string title)
    {
        ClearScreen();
        AnsiConsole.Write(new Rule($"[bold yellow]⚽ {Markup.Escape(title)}[/]")
            .LeftJustified()
            .RuleStyle("green"));
        AnsiConsole.WriteLine();
    }

    // Shows a menu and returns 1..n for the chosen option, or 0 for Back/Exit.
    public static int Menu(string backLabel, params string[] options)
    {
        if (IsInteractive)
        {
            var items = options.Select((text, i) => $"{i + 1}. {text}")
                .Append($"0. {backLabel}")
                .ToList();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Use [bold]↑ ↓[/] and press [bold]Enter[/][/]")
                    .HighlightStyle(new Style(Color.Black, Color.Green))
                    .PageSize(10)
                    .AddChoices(items));

            return int.Parse(choice.Split('.')[0]);
        }

        for (var i = 0; i < options.Length; i++)
            AnsiConsole.MarkupLine($"  [green]{i + 1}.[/] {Markup.Escape(options[i])}");
        AnsiConsole.MarkupLine($"  [grey]0. {Markup.Escape(backLabel)}[/]");

        return ReadInt("Choose an option:", 0, options.Length);
    }

    public static Table CreateTable(params string[] headers)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green);

        foreach (var header in headers)
            table.AddColumn(new TableColumn($"[bold yellow]{Markup.Escape(header)}[/]"));

        return table;
    }

    /* ==================== Reading input ==================== */

    public static string ReadRequiredString(string prompt, int maxLength)
    {
        while (true)
        {
            var value = Ask(prompt);

            if (value.Length == 0)
                Error("Value cannot be empty.");
            else if (value.Length > maxLength)
                Error($"Value cannot be longer than {maxLength} characters.");
            else
                return value;
        }
    }

    // Returns null when the user presses Enter (keep current value)
    public static string? ReadOptionalString(string prompt, int maxLength)
    {
        while (true)
        {
            var value = Ask(prompt);

            if (value.Length == 0)
                return null;
            if (value.Length > maxLength)
                Error($"Value cannot be longer than {maxLength} characters.");
            else
                return value;
        }
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            if (int.TryParse(Ask(prompt), out var value) && value >= min && value <= max)
                return value;

            Error(max == int.MaxValue
                ? $"Please enter a whole number of at least {min}."
                : $"Please enter a whole number between {min} and {max}.");
        }
    }

    // Returns null when the user presses Enter (keep current value)
    public static int? ReadOptionalInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            var text = Ask(prompt);

            if (text.Length == 0)
                return null;
            if (int.TryParse(text, out var value) && value >= min && value <= max)
                return value;

            Error("Please enter a valid number, or press Enter to skip.");
        }
    }

    // Returns null when the user presses Enter (keep current / default value)
    public static decimal? ReadOptionalDecimal(string prompt, decimal min, decimal max)
    {
        while (true)
        {
            var text = Ask(prompt).Replace("$", "").Replace(",", "");

            if (text.Length == 0)
                return null;
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                && value >= min && value <= max)
                return Math.Round(value, 2);

            Error($"Please enter an amount between {min:C} and {max:C}, or press Enter to skip.");
        }
    }

    public static int ReadId(string entityName) =>
        ReadInt($"Enter {entityName} ID:", 1);

    public static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            var value = ReadOptionalDate(prompt);
            if (value.HasValue)
                return value.Value;

            Error("Date cannot be empty.");
        }
    }

    // Returns null when the user presses Enter (keep current value)
    public static DateTime? ReadOptionalDate(string prompt)
    {
        while (true)
        {
            var text = Ask(prompt);

            if (text.Length == 0)
                return null;

            if (DateTime.TryParseExact(text, new[] { DateFormat, "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            Error($"Invalid date. Use the format {DateFormat} (for example 2026-10-20 17:30).");
        }
    }

    public static bool Confirm(string question)
    {
        if (IsInteractive)
            return AnsiConsole.Confirm($"[yellow]{Markup.Escape(question)}[/]", defaultValue: false);

        var answer = Ask($"{question} (y/n):").ToLowerInvariant();
        return answer is "y" or "yes";
    }

    private static string Ask(string prompt)
    {
        AnsiConsole.Markup($"[deepskyblue1]{Markup.Escape(prompt)}[/] ");

        // ReadLine returns null when the input stream is closed (e.g. piped input ended).
        // Stop instead of looping forever on validation errors.
        var line = Console.ReadLine()
            ?? throw new EndOfStreamException("Input stream closed.");
        return line.Trim();
    }

    /* ==================== Messages ==================== */

    public static void ShowResult(OperationResult result)
    {
        if (result.Success)
            Success(result.Message);
        else
            Error(result.Message);
    }

    public static void Success(string message) =>
        AnsiConsole.MarkupLine($"[bold green]✔ {Markup.Escape(message)}[/]");

    public static void Error(string message) =>
        AnsiConsole.MarkupLine($"[bold red]✖ {Markup.Escape(message)}[/]");

    public static void Info(string message) =>
        AnsiConsole.MarkupLine($"[yellow]ℹ {Markup.Escape(message)}[/]");

    public static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[grey]Press Enter to continue...[/]");
        if (Console.ReadLine() is null)
            throw new EndOfStreamException("Input stream closed.");
    }

    // Console.Clear fails when there is no real console window
    public static void ClearScreen()
    {
        try
        {
            if (!Console.IsOutputRedirected)
                AnsiConsole.Clear();
        }
        catch (IOException)
        {
            // No real console window - just continue without clearing
        }
    }
}
