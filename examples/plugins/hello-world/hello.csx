#!/usr/bin/env dotnet-script
#r "nuget: Spectre.Console, 0.50.0"

using Spectre.Console;

// Simple Hello World plugin example
AnsiConsole.MarkupLine("[bold green]Hello from CLI Dashboard![/]");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("This is a [italic]simple example plugin[/] that demonstrates:");
AnsiConsole.WriteLine();

var table = new Table();
table.Border(TableBorder.Rounded);
table.AddColumn("Feature");
table.AddColumn("Description");

table.AddRow("[cyan]Plugin Structure[/]", "YAML metadata + CSX script");
table.AddRow("[cyan]Spectre.Console[/]", "Rich console output with colors and formatting");
table.AddRow("[cyan]Easy to Create[/]", "Just add YAML + script to plugin folder");

AnsiConsole.Write(table);
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
Console.ReadKey(true);
