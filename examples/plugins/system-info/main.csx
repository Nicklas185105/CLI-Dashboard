#!/usr/bin/env dotnet-script
#r "nuget: Spectre.Console, 0.50.0"
#r "nuget: Serilog, 4.3.0"
#r "../../CliDashboard.Shared.dll"

using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;
using CliDashboard.Shared.Utils;
using System.Runtime.InteropServices;

ConsoleUtils.Clear();

AnsiConsole.Write(
    new FigletText("System Info")
        .LeftJustified()
        .Color(Color.Cyan1));

AnsiConsole.WriteLine();

// Operating System Information
var osTable = new Table();
osTable.Border(TableBorder.Rounded);
osTable.AddColumn(new TableColumn("[bold]Property[/]").Centered());
osTable.AddColumn(new TableColumn("[bold]Value[/]"));

osTable.AddRow("[cyan]OS[/]", RuntimeInformation.OSDescription);
osTable.AddRow("[cyan]Architecture[/]", RuntimeInformation.OSArchitecture.ToString());
osTable.AddRow("[cyan].NET Runtime[/]", RuntimeInformation.FrameworkDescription);
osTable.AddRow("[cyan]Machine Name[/]", Environment.MachineName);
osTable.AddRow("[cyan]User Name[/]", Environment.UserName);
osTable.AddRow("[cyan]System Uptime[/]", TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss"));

AnsiConsole.Write(
    new Panel(osTable)
        .Header("[yellow]Operating System[/]")
        .BorderColor(Color.Yellow));

AnsiConsole.WriteLine();

// CPU Information
var cpuTable = new Table();
cpuTable.Border(TableBorder.Rounded);
cpuTable.AddColumn(new TableColumn("[bold]Property[/]").Centered());
cpuTable.AddColumn(new TableColumn("[bold]Value[/]"));

cpuTable.AddRow("[green]Processor Count[/]", Environment.ProcessorCount.ToString());
cpuTable.AddRow("[green]Is 64-bit OS[/]", Environment.Is64BitOperatingSystem.ToString());
cpuTable.AddRow("[green]Is 64-bit Process[/]", Environment.Is64BitProcess.ToString());

AnsiConsole.Write(
    new Panel(cpuTable)
        .Header("[green]Processor[/]")
        .BorderColor(Color.Green));

AnsiConsole.WriteLine();

// Memory Information
var process = Process.GetCurrentProcess();
var memoryTable = new Table();
memoryTable.Border(TableBorder.Rounded);
memoryTable.AddColumn(new TableColumn("[bold]Property[/]").Centered());
memoryTable.AddColumn(new TableColumn("[bold]Value[/]"));

memoryTable.AddRow("[blue]Working Set[/]", $"{process.WorkingSet64 / 1024 / 1024:N0} MB");
memoryTable.AddRow("[blue]Private Memory[/]", $"{process.PrivateMemorySize64 / 1024 / 1024:N0} MB");
memoryTable.AddRow("[blue]Virtual Memory[/]", $"{process.VirtualMemorySize64 / 1024 / 1024:N0} MB");

AnsiConsole.Write(
    new Panel(memoryTable)
        .Header("[blue]Memory (Current Process)[/]")
        .BorderColor(Color.Blue));

AnsiConsole.WriteLine();

// Disk Information
var diskTable = new Table();
diskTable.Border(TableBorder.Rounded);
diskTable.AddColumn(new TableColumn("[bold]Drive[/]").Centered());
diskTable.AddColumn(new TableColumn("[bold]Type[/]").Centered());
diskTable.AddColumn(new TableColumn("[bold]Total Size[/]").RightAligned());
diskTable.AddColumn(new TableColumn("[bold]Free Space[/]").RightAligned());
diskTable.AddColumn(new TableColumn("[bold]Usage[/]").Centered());

foreach (var drive in DriveInfo.GetDrives())
{
    if (drive.IsReady)
    {
        var totalGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
        var freeGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
        var usedPercent = (1 - (freeGB / totalGB)) * 100;

        var barColor = usedPercent > 90 ? Color.Red :
                       usedPercent > 75 ? Color.Yellow :
                       Color.Green;

        var usageBar = new BreakdownChart()
            .Width(20)
            .AddItem("Used", usedPercent, barColor)
            .AddItem("Free", 100 - usedPercent, Color.Grey);

        diskTable.AddRow(
            $"[purple]{drive.Name}[/]",
            drive.DriveType.ToString(),
            $"{totalGB:F2} GB",
            $"{freeGB:F2} GB",
            $"{usedPercent:F1}%"
        );
    }
}

AnsiConsole.Write(
    new Panel(diskTable)
        .Header("[purple]Disk Drives[/]")
        .BorderColor(Color.Purple));