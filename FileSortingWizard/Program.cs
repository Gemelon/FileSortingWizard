using FileSortingWiz;
using System.CommandLine;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Command = System.CommandLine.Command;

namespace FileSortingWizard
{
    internal class Program
    {
        private static Wizard wizard = new Wizard();

        static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand("TSFileWizard - A tool to sort files on the HDD");
            Command executeCommand = new Command("execute", "Execute the rule file (Collection and Sorting)");
            Command collectCommand = new Command("collect", "Execute the collection of file informations in to the database");
            Command sortCommand = new Command("sort", "Executes the file sorting");

            Option<bool> silentOption = new Option<bool>("--silent", "-s")
            {
                Description = "Prevents the output of program messages",
                HelpName = "Silent",
                Required = false, // Set IsRequired to true for the silent option
                DefaultValueFactory = _ => false // Correctly use a lambda to provide a default value
            };

            Option<string> rulesFileOption = new Option<string>("--rules", "-r")
            {
                Description = "Path to the rule file",
                HelpName = "Rule File",
                Required = true // Set IsRequired to true for the rule option
            };

            Option<string> inputDirectoryOption = new Option<string>("--input", "-i")
            {
                Description = "Path to the input directory",
                HelpName = "Input Directory",
                Required = true,
                //DefaultValueFactory = _ => string.Empty // Correctly use a lambda to provide a default value
            };

            Option<string> outputDirectoryOption = new Option<string>("--output", "-o")
            {
                Description = "Path to the output directory",
                HelpName = "Output Directory",
                Required = false,
                DefaultValueFactory = _ => string.Empty // Correctly use a lambda to provide a default value
            };

            executeCommand.SetAction(parseResult =>
            {
                string? ruleFileName = parseResult.GetValue<string>("--rules");
                string? inputDirectory = parseResult.GetValue<string>("--input");
                string? outputDirectory = parseResult.GetValue<string>("--output");
                bool? silent = parseResult.GetValue<bool>("--silent");
                ExecuteRules(ruleFileName, inputDirectory, outputDirectory, silent);
            });

            collectCommand.SetAction(parseResult =>
            {
                string? ruleFileName = parseResult.GetValue<string>("--rules");
                string? inputDirectory = parseResult.GetValue<string>("--input");
                string? outputDirectory = parseResult.GetValue<string>("--output");
                bool? silent = parseResult.GetValue<bool>("--silent");
                ExecuteCollection(ruleFileName, inputDirectory, outputDirectory, silent);
            });

            sortCommand.SetAction(parseResult =>
            {
                string? ruleFileName = parseResult.GetValue<string>("--rules");
                string? inputDirectory = parseResult.GetValue<string>("--input");
                string? outputDirectory = parseResult.GetValue<string>("--output");
                bool? silent = parseResult.GetValue<bool>("--silent");
                ExecuteSorting(ruleFileName, inputDirectory, outputDirectory, silent);
            });

            executeCommand.Add(rulesFileOption);
            executeCommand.Add(inputDirectoryOption);
            executeCommand.Add(outputDirectoryOption);
            executeCommand.Add(silentOption);
            rootCommand.Add(executeCommand);

            collectCommand.Add(rulesFileOption);
            collectCommand.Add(inputDirectoryOption);
            collectCommand.Add(outputDirectoryOption);
            collectCommand.Add(silentOption);
            rootCommand.Add(collectCommand);

            sortCommand.Add(rulesFileOption);
            sortCommand.Add(inputDirectoryOption);
            sortCommand.Add(outputDirectoryOption);
            sortCommand.Add(silentOption);
            rootCommand.Add(sortCommand);

            ParseResult parseResult = rootCommand.Parse(args);

            return parseResult.Invoke();
        }
        private static void ExecuteRules(string? ruleFileName, string? inputDirectory, string? outputDirectory, bool? silent)
        {
            //wizard.ExecuteRules(ruleFileName ?? string.Empty, inputDirectory ?? string.Empty, outputDirectory ?? string.Empty);
        }

        private static void ExecuteCollection(string? ruleFileName, string? inputDirectory, string? outputDirectory, bool? silent)
        {
            wizard.CollectFileData(ruleFileName ?? string.Empty, inputDirectory ?? string.Empty, outputDirectory ?? string.Empty, silent ?? false);
        }

        private static void ExecuteSorting(string? ruleFileName, string? inputDirectory, string? outputDirectory, bool? silent)
        {
            //wizard.SortFiles(ruleFileName ?? string.Empty, inputDirectory ?? string.Empty, outputDirectory ?? string.Empty);
        }
    }
}
