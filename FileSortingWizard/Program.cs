/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2025 Thoms Stoll
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

/********************************************************************************
 * FileSortingWizard
 * A file sorting wizard to organize your files based on customizable rules.
 ********************************************************************************/

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
			Command copyCommand = new Command("copy", "copyies the files to the output directory");
			Command moveCommand = new Command("move", "moves the files to the output directory)");
			//Command executeCommand = new Command("execute", "Execute the rule file (Collection and Sorting)");
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

			copyCommand.SetAction(parseResult =>
			{
				string? ruleFileName = parseResult.GetValue<string>("--rules");
				string? outputDirectory = parseResult.GetValue<string>("--output");
				bool? silent = parseResult.GetValue<bool>("--silent");
				ExecuteCopy(ruleFileName, outputDirectory, silent);
			});

			moveCommand.SetAction(parseResult =>
			{
				string? ruleFileName = parseResult.GetValue<string>("--rules");
				string? outputDirectory = parseResult.GetValue<string>("--output");
				bool? silent = parseResult.GetValue<bool>("--silent");
				ExecuteMove(ruleFileName, outputDirectory, silent);
			});

			//executeCommand.SetAction(parseResult =>
			//{
			//	string? ruleFileName = parseResult.GetValue<string>("--rules");
			//	string? inputDirectory = parseResult.GetValue<string>("--input");
			//	string? outputDirectory = parseResult.GetValue<string>("--output");
			//	bool? silent = parseResult.GetValue<bool>("--silent");
			//	ExecuteRules(ruleFileName, inputDirectory, outputDirectory, silent);
			//});

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

			copyCommand.Add(rulesFileOption);
			copyCommand.Add(outputDirectoryOption);
			copyCommand.Add(silentOption);
			rootCommand.Add(copyCommand);

			moveCommand.Add(rulesFileOption);
			moveCommand.Add(outputDirectoryOption);
			moveCommand.Add(silentOption);
			rootCommand.Add(moveCommand);

			//executeCommand.Add(rulesFileOption);
			//executeCommand.Add(inputDirectoryOption);
			//executeCommand.Add(outputDirectoryOption);
			//executeCommand.Add(silentOption);
			//rootCommand.Add(executeCommand);

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
		private static void ExecuteCopy(string? ruleFileName, string? outputDirectory, bool? silent)
		{
			wizard.ExecuteCopy(ruleFileName ?? string.Empty, outputDirectory ?? string.Empty);
		}

		private static void ExecuteMove(string? ruleFileName, string? outputDirectory, bool? silent)
		{
			wizard.ExecuteMove(ruleFileName ?? string.Empty, outputDirectory ?? string.Empty);
		}

		//private static void ExecuteRules(string? ruleFileName, string? inputDirectory, string? outputDirectory, bool? silent)
		//{
		//	//wizard.ExecuteRules(ruleFileName ?? string.Empty, inputDirectory ?? string.Empty, outputDirectory ?? string.Empty);
		//}

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
