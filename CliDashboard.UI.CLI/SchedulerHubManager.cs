namespace CliDashboard.UI.CLI;

internal class SchedulerHubManager(
    TaskSchedulerService schedulerService,
    PluginManager pluginManager,
    ScriptManager scriptManager)
{
    public void ShowMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Scheduled Tasks").Centered().Color(Color.Yellow));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Task Scheduler[/]")
                    .AddChoices(new[]
                    {
                        "📋 View All Tasks",
                        "➕ Create New Task",
                        "📊 View Task History",
                        "⚙️  Scheduler Status",
                        "◀️  Back"
                    }));

            switch (choice)
            {
                case "📋 View All Tasks":
                    ViewAllTasks();
                    break;
                case "➕ Create New Task":
                    CreateTask();
                    break;
                case "📊 View Task History":
                    ViewTaskHistory();
                    break;
                case "⚙️  Scheduler Status":
                    ViewSchedulerStatus();
                    break;
                case "◀️  Back":
                    return;
            }
        }
    }

    private void ViewAllTasks()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[yellow]Scheduled Tasks[/]\n");

            var tasks = schedulerService.GetAllTasks();

            if (tasks.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No scheduled tasks.[/]");
                ConsoleUtils.PauseForUser();
                return;
            }

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Type");
            table.AddColumn("Schedule");
            table.AddColumn("Next Run");
            table.AddColumn("Status");
            table.AddColumn("Last Status");

            foreach (var taskItem in tasks.OrderBy(t => t.NextExecutionTime))
            {
                var status = taskItem.Enabled ? "[green]Enabled[/]" : "[grey]Disabled[/]";
                var lastStatus = taskItem.LastStatus switch
                {
                    TaskExecutionStatus.Success => "[green]✓[/]",
                    TaskExecutionStatus.Failed => "[red]✗[/]",
                    _ => "[grey]-[/]"
                };
                var nextRun = taskItem.NextExecutionTime?.ToString("MM/dd HH:mm") ?? "[grey]-[/]";
                var schedule = SimpleCronParser.GetHumanReadable(taskItem.CronExpression);

                table.AddRow(
                    taskItem.Name,
                    taskItem.Type.ToString(),
                    schedule,
                    nextRun,
                    status,
                    lastStatus);
            }

            AnsiConsole.Write(table);

            var taskChoices = tasks.Select(t => t.Name).ToList();
            taskChoices.Add("Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]Select a task to manage:[/]")
                    .PageSize(15)
                    .AddChoices(taskChoices));

            if (selected == "Back")
                return;

            var task = tasks.First(t => t.Name == selected);
            ManageTask(task);
        }
    }

    private void ManageTask(ScheduledTask task)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[yellow]{task.Name}[/]\n");

            var table = new Table();
            table.AddColumn("Property");
            table.AddColumn("Value");

            table.AddRow("Type", task.Type.ToString());
            table.AddRow("Target", task.Target);
            table.AddRow("Schedule", SimpleCronParser.GetHumanReadable(task.CronExpression));
            table.AddRow("Cron Expression", task.CronExpression);
            table.AddRow("Enabled", task.Enabled ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Next Execution", task.NextExecutionTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "[grey]-[/]");
            table.AddRow("Last Execution", task.LastExecutionTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "[grey]-[/]");
            table.AddRow("Last Status", task.LastStatus?.ToString() ?? "[grey]-[/]");
            table.AddRow("Execution Count", task.ExecutionCount.ToString());
            table.AddRow("Notify on Completion", task.NotifyOnCompletion ? "Yes" : "No");
            table.AddRow("Notify on Failure", task.NotifyOnFailure ? "Yes" : "No");

            AnsiConsole.Write(table);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]Choose an action:[/]")
                    .AddChoices(new[]
                    {
                        task.Enabled ? "Disable Task" : "Enable Task",
                        "Edit Task",
                        "View Task History",
                        "Delete Task",
                        "Back"
                    }));

            switch (choice)
            {
                case "Enable Task":
                case "Disable Task":
                    schedulerService.ToggleTask(task.Id);
                    AnsiConsole.MarkupLine($"[green]Task {(task.Enabled ? "disabled" : "enabled")}![/]");
                    ConsoleUtils.PauseForUser();
                    return;
                case "Edit Task":
                    EditTask(task);
                    return;
                case "View Task History":
                    ViewTaskHistory(task.Id);
                    break;
                case "Delete Task":
                    if (AnsiConsole.Confirm($"Are you sure you want to delete '{task.Name}'?"))
                    {
                        schedulerService.DeleteTask(task.Id);
                        AnsiConsole.MarkupLine("[green]Task deleted![/]");
                        ConsoleUtils.PauseForUser();
                        return;
                    }
                    break;
                case "Back":
                    return;
            }
        }
    }

    private void CreateTask()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[yellow]Create New Scheduled Task[/]\n");

        var name = AnsiConsole.Ask<string>("Task name:");

        var type = AnsiConsole.Prompt(
            new SelectionPrompt<TaskType>()
                .Title("Task type:")
                .AddChoices(TaskType.Plugin, TaskType.Script));

        string target;
        if (type == TaskType.Plugin)
        {
            var plugins = pluginManager.LoadPlugins().SelectMany(p => p.Value).Select(p => p.Name).ToList();
            if (plugins.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No plugins available![/]");
                ConsoleUtils.PauseForUser();
                return;
            }
            target = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select plugin:")
                    .AddChoices(plugins));
        }
        else
        {
            var scripts = scriptManager.LoadScripts().Select(s => s.Name).ToList();
            if (scripts.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No scripts available![/]");
                ConsoleUtils.PauseForUser();
                return;
            }
            target = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select script:")
                    .AddChoices(scripts));
        }

        AnsiConsole.MarkupLine("\n[cyan]Common cron patterns:[/]");
        AnsiConsole.MarkupLine("  [grey]*/5 * * * *[/]  - Every 5 minutes");
        AnsiConsole.MarkupLine("  [grey]0 * * * *[/]    - Every hour");
        AnsiConsole.MarkupLine("  [grey]0 9 * * *[/]    - Daily at 9 AM");
        AnsiConsole.MarkupLine("  [grey]0 9 * * 1[/]    - Every Monday at 9 AM");
        AnsiConsole.MarkupLine("  [grey]0 0 1 * *[/]    - First day of month at midnight\n");

        string cronExpression;
        while (true)
        {
            cronExpression = AnsiConsole.Ask<string>("Cron expression:");
            if (SimpleCronParser.ValidateCronExpression(cronExpression))
            {
                var readable = SimpleCronParser.GetHumanReadable(cronExpression);
                AnsiConsole.MarkupLine($"[green]✓ Valid: {readable}[/]");
                break;
            }
            AnsiConsole.MarkupLine("[red]Invalid cron expression. Try again.[/]");
        }

        var notifyOnCompletion = AnsiConsole.Confirm("Send notification on completion?", false);
        var notifyOnFailure = AnsiConsole.Confirm("Send notification on failure?", true);

        var task = new ScheduledTask
        {
            Name = name,
            Type = type,
            Target = target,
            CronExpression = cronExpression,
            NotifyOnCompletion = notifyOnCompletion,
            NotifyOnFailure = notifyOnFailure
        };

        schedulerService.AddTask(task);
        AnsiConsole.MarkupLine("\n[green]✓ Task created successfully![/]");
        AnsiConsole.MarkupLine($"[grey]Next execution: {task.NextExecutionTime}[/]");
        ConsoleUtils.PauseForUser();
    }

    private void EditTask(ScheduledTask task)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[yellow]Edit Task: {task.Name}[/]\n");

        task.Name = AnsiConsole.Ask("Task name:", task.Name);

        AnsiConsole.MarkupLine($"\n[grey]Current schedule: {SimpleCronParser.GetHumanReadable(task.CronExpression)}[/]");
        if (AnsiConsole.Confirm("Change schedule?"))
        {
            string cronExpression;
            while (true)
            {
                cronExpression = AnsiConsole.Ask("Cron expression:", task.CronExpression);
                if (SimpleCronParser.ValidateCronExpression(cronExpression))
                {
                    task.CronExpression = cronExpression;
                    break;
                }
                AnsiConsole.MarkupLine("[red]Invalid cron expression. Try again.[/]");
            }
        }

        task.NotifyOnCompletion = AnsiConsole.Confirm("Notify on completion?", task.NotifyOnCompletion);
        task.NotifyOnFailure = AnsiConsole.Confirm("Notify on failure?", task.NotifyOnFailure);

        schedulerService.UpdateTask(task);
        AnsiConsole.MarkupLine("\n[green]✓ Task updated![/]");
        ConsoleUtils.PauseForUser();
    }

    private void ViewTaskHistory(string? taskId = null)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[yellow]Task Execution History[/]\n");

        var history = schedulerService.GetTaskHistory(taskId, 20);

        if (history.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No execution history.[/]");
            ConsoleUtils.PauseForUser();
            return;
        }

        var table = new Table();
        table.AddColumn("Task");
        table.AddColumn("Execution Time");
        table.AddColumn("Status");
        table.AddColumn("Duration");

        foreach (var entry in history)
        {
            var status = entry.Status switch
            {
                TaskExecutionStatus.Success => "[green]Success[/]",
                TaskExecutionStatus.Failed => "[red]Failed[/]",
                _ => "[grey]Unknown[/]"
            };

            table.AddRow(
                entry.TaskName,
                entry.ExecutionTime.ToString("MM/dd HH:mm:ss"),
                status,
                $"{entry.DurationMs}ms");
        }

        AnsiConsole.Write(table);
        ConsoleUtils.PauseForUser();
    }

    private void ViewSchedulerStatus()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[yellow]Scheduler Status[/]\n");

        var tasks = schedulerService.GetAllTasks();
        var enabledTasks = tasks.Count(t => t.Enabled);
        var upcomingTasks = tasks.Where(t => t.Enabled && t.NextExecutionTime.HasValue)
            .OrderBy(t => t.NextExecutionTime)
            .Take(5)
            .ToList();

        var table = new Table();
        table.AddColumn("Metric");
        table.AddColumn("Value");

        table.AddRow("Total Tasks", tasks.Count.ToString());
        table.AddRow("Enabled Tasks", $"[green]{enabledTasks}[/]");
        table.AddRow("Disabled Tasks", $"[grey]{tasks.Count - enabledTasks}[/]");
        table.AddRow("Scheduler Status", "[green]Running[/]");

        AnsiConsole.Write(table);

        if (upcomingTasks.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[cyan]Upcoming Executions:[/]");
            foreach (var task in upcomingTasks)
            {
                AnsiConsole.MarkupLine($"  • {task.Name}: [grey]{task.NextExecutionTime:MM/dd HH:mm}[/]");
            }
        }

        ConsoleUtils.PauseForUser();
    }
}
