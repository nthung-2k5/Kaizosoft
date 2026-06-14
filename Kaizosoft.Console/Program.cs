using System.Text;
using Kaizosoft.Console;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length != 0) { CommandLine.Run(args); }
else
{
    AnsiConsole.MarkupLine("[bold blue]Kaizosoft Game Translation Tool[/]");
    AnsiConsole.WriteLine();

    Directory.CreateDirectory(ConsoleInteractive.INPUT_DIRECTORY);
    Directory.CreateDirectory(ConsoleInteractive.OUTPUT_DIRECTORY);

    ConsoleInteractive.Run();

    AnsiConsole.Write("Press any key to close...");
    AnsiConsole.Console.Input.ReadKey(false);
}
