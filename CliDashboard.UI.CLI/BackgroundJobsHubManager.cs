namespace CliDashboard.UI.CLI;

internal class BackgroundJobsHubManager(
    BackgroundJobManager jobManager,
    PluginManager pluginManager,
    ScriptManager scriptManager)
{
    public void ShowMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Background Jobs").Centered().Color(Color.Purple));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Background Job Manager[/]")
                    .AddChoices(new[]
                    {
                        "📋 View Running Jobs",
                        "🚀 Start New Job",
                        "📊 View All Jobs",
                        "◀️  Back"
                    }));

            switch (choice)
            {
                case "📋 View Running Jobs":
                    ViewRunningJobs();
                    break;
                case "🚀 Start New Job":
                    StartNewJob();
                    break;
                case "📊 View All Jobs":
                    ViewAllJobs();
                    break;
                case "◀️  Back":
                    return;
            }
        }
    }

    private void ViewRunningJobs()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[purple]Running Background Jobs[/]\n");

            var jobs = jobManager.GetAllJobs().Where(j => j.Status == JobStatus.Running).ToList();

            if (jobs.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No running jobs.[/]");
                ConsoleUtils.PauseForUser();
                return;
            }

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Started");
            table.AddColumn("Duration");
            table.AddColumn("Status");

            foreach (var jobItem in jobs.OrderBy(j => j.StartTime))
            {
                table.AddRow(
                    jobItem.Name,
                    jobItem.StartTime.ToString("HH:mm:ss"),
                    FormatDuration(jobItem.Duration),
                    "[green]Running[/]");
            }

            AnsiConsole.Write(table);

            var jobChoices = jobs.Select(j => j.Name).ToList();
            jobChoices.Add("Refresh");
            jobChoices.Add("Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]Select a job to manage:[/]")
                    .AddChoices(jobChoices));

            if (selected == "Back")
                return;

            if (selected == "Refresh")
                continue;

            var job = jobs.First(j => j.Name == selected);
            ManageJob(job);
        }
    }

    private void ViewAllJobs()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[purple]All Background Jobs[/]\n");

            var jobs = jobManager.GetAllJobs().OrderByDescending(j => j.StartTime).ToList();

            if (jobs.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No jobs.[/]");
                ConsoleUtils.PauseForUser();
                return;
            }

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Started");
            table.AddColumn("Duration");
            table.AddColumn("Status");
            table.AddColumn("Exit Code");

            foreach (var jobItem in jobs)
            {
                var status = jobItem.Status switch
                {
                    JobStatus.Running => "[green]Running[/]",
                    JobStatus.Completed => "[blue]Completed[/]",
                    JobStatus.Failed => "[red]Failed[/]",
                    JobStatus.Stopped => "[yellow]Stopped[/]",
                    _ => "[grey]Unknown[/]"
                };

                table.AddRow(
                    jobItem.Name,
                    jobItem.StartTime.ToString("MM/dd HH:mm"),
                    FormatDuration(jobItem.Duration),
                    status,
                    jobItem.ExitCode?.ToString() ?? "[grey]-[/]");
            }

            AnsiConsole.Write(table);

            var jobChoices = jobs.Select(j => j.Name).ToList();
            jobChoices.Add("Refresh");
            jobChoices.Add("Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]Select a job:[/]")
                    .PageSize(15)
                    .AddChoices(jobChoices));

            if (selected == "Back")
                return;

            if (selected == "Refresh")
                continue;

            var job = jobs.First(j => j.Name == selected);
            ManageJob(job);
        }
    }

    private void ManageJob(BackgroundJob job)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[purple]{job.Name}[/]\n");

        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Job ID", job.Id);
        table.AddRow("Command", job.Command);
        table.AddRow("Arguments", job.Arguments);
        table.AddRow("Working Directory", job.WorkingDirectory ?? "[grey]-[/]");
        table.AddRow("Start Time", job.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("End Time", job.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "[grey]-[/]");
        table.AddRow("Duration", FormatDuration(job.Duration));
        table.AddRow("Status", job.Status.ToString());
        table.AddRow("Exit Code", job.ExitCode?.ToString() ?? "[grey]-[/]");

        AnsiConsole.Write(table);

        var choices = new List<string>();
        if (job.Status == JobStatus.Running)
        {
            choices.Add("Stop Job");
        }
        if (!string.IsNullOrEmpty(job.Output))
        {
            choices.Add("View Output");
        }
        if (!string.IsNullOrEmpty(job.Error))
        {
            choices.Add("View Errors");
        }
        choices.Add("Remove from List");
        choices.Add("Back");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[grey]Choose an action:[/]")
                .AddChoices(choices));

        switch (choice)
        {
            case "Stop Job":
                if (AnsiConsole.Confirm($"Are you sure you want to stop '{job.Name}'?"))
                {
                    jobManager.StopJob(job.Id);
                    AnsiConsole.MarkupLine("[yellow]Job stopped![/]");
                    ConsoleUtils.PauseForUser();
                }
                break;

            case "View Output":
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"[cyan]Output from {job.Name}:[/]\n");
                AnsiConsole.Write(new Panel(job.Output ?? "")
                    .BorderColor(Color.Blue)
                    .Header("Standard Output"));
                ConsoleUtils.PauseForUser();
                break;

            case "View Errors":
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"[red]Errors from {job.Name}:[/]\n");
                AnsiConsole.Write(new Panel(job.Error ?? "")
                    .BorderColor(Color.Red)
                    .Header("Standard Error"));
                ConsoleUtils.PauseForUser();
                break;

            case "Remove from List":
                if (job.Status == JobStatus.Running)
                {
                    AnsiConsole.MarkupLine("[yellow]Cannot remove running job. Stop it first.[/]");
                    ConsoleUtils.PauseForUser();
                }
                else if (AnsiConsole.Confirm($"Remove '{job.Name}' from list?"))
                {
                    jobManager.RemoveJob(job.Id);
                    AnsiConsole.MarkupLine("[green]Job removed from list![/]");
                    ConsoleUtils.PauseForUser();
                }
                break;
        }
    }

    private void StartNewJob()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[purple]Start New Background Job[/]\n");

        var jobName = AnsiConsole.Ask<string>("Job name:");

        var type = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to run?")
                .AddChoices("Plugin", "Script", "Custom Command"));

        string command;
        string arguments = "";

        switch (type)
        {
            case "Plugin":
                var plugins = pluginManager.LoadPlugins().SelectMany(p => p.Value).Select(p => p.Name).ToList();
                if (plugins.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No plugins available![/]");
                    ConsoleUtils.PauseForUser();
                    return;
                }
                var pluginName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select plugin:")
                        .AddChoices(plugins));

                var plugin = pluginManager.LoadPlugins().SelectMany(p => p.Value)
                    .First(p => p.Name == pluginName);

                command = "dotnet";
                arguments = $"script \"{plugin.ScriptPath}\"";
                break;

            case "Script":
                var scripts = scriptManager.LoadScripts().Select(s => s.Name).ToList();
                if (scripts.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No scripts available![/]");
                    ConsoleUtils.PauseForUser();
                    return;
                }
                var scriptName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select script:")
                        .AddChoices(scripts));

                var script = scriptManager.LoadScripts().First(s => s.Name == scriptName);

                if (script.Path.EndsWith(".csx"))
                {
                    command = "dotnet";
                    arguments = $"script \"{script.Path}\"";
                }
                else
                {
                    command = "powershell";
                    arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{script.Path}\"";
                }
                break;

            case "Custom Command":
                command = AnsiConsole.Ask<string>("Command:");
                arguments = AnsiConsole.Ask<string>("Arguments (optional):", "");
                break;

            default:
                return;
        }

        var workingDir = AnsiConsole.Ask<string>("Working directory (optional, press Enter to skip):", "");
        if (string.IsNullOrWhiteSpace(workingDir))
            workingDir = null;

        try
        {
            var jobId = jobManager.StartJob(jobName, command, arguments, workingDir);
            AnsiConsole.MarkupLine($"\n[green]✓ Background job started![/]");
            AnsiConsole.MarkupLine($"[grey]Job ID: {jobId}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to start job: {ex.Message}[/]");
        }

        ConsoleUtils.PauseForUser();
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        return $"{duration.Seconds}s";
    }
}
