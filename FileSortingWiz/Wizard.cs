
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

using SharpExifTool;
using Spectre.Console;
using System.Globalization;
using System.Reflection;
using YamlDotNet.Core.Tokens;

namespace FileSortingWiz
{
	/// <summary>
	/// Represents a wizard that processes rule files and manages file data collection.
	/// </summary>
	/// <remarks>The <see cref="Wizard"/> class provides functionality to initialize components and collect
	/// file data based on a specified rule file and directory paths. It is designed to handle operations related to
	/// rule file deserialization and logging.</remarks>
	public class Wizard
	{
		private Lib? _lib;
		private RuleFile? _ruleFile;
		private DataBase? _database;

		private TextReader? _ruleFileReader;
		private bool _silent;
		private string? _dataBaseFileName;
		private int _fileCount;

		/// <summary>
		/// Initializes the components required for processing based on the specified rule file.
		/// </summary>
		/// <remarks>This method initializes the rule file, logging library, and database components. It deserializes
		/// the rule file, configures logging, and sets up the database using the specified rule file's
		/// configuration.</remarks>
		/// <param name="ruleFileName">The path to the rule file used to configure the components. The file must exist and be in a valid format.</param>
		private void InitializeComponents(string ruleFileName)
		{
			// <exception cref="ArgumentNullException">Thrown when <paramref name="ruleFileName"/> is null.</exception>
			if (ruleFileName == null)
			{
				throw new ArgumentNullException(nameof(ruleFileName));
			}

			_ruleFile = new RuleFile();
			_lib = new Lib();
			_database = new DataBase();

			// Deserialize rule file
			_ruleFile.Deserialize(ruleFileName);

			// Initialize log file
			_lib.ResetLog(_ruleFile.rule.logFileName);

			// Initialize database
			_dataBaseFileName = Path.GetFullPath(_ruleFile.rule.DataBasePath + "\\" + _ruleFile.rule.DataBaseFileName);
			_lib.WriteMessage($"DataBase path: {_dataBaseFileName}", _silent);
			_database.InitializeDataBase(_dataBaseFileName);
		}

		public void CollectFileData(string ruleFileName, string inputDirectory, string outputDirectory, bool silent)
		{
			ExifTool exifTool = new SharpExifTool.ExifTool();
			FileMetaInfo fileMetaInfo = new FileMetaInfo();
			List<string> NoMetadataRule = new List<string>();
			PropertyInfo propertyInfo;
			DateTime currentDateTimeValue = DateTime.MinValue;

			var value = "";
			string lastField = "";

			_silent = silent;

			InitializeComponents(ruleFileName);

			string[] directories = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
			Array.Sort(directories);

			_lib.WriteMessage($"Collecting file data from {inputDirectory} with {directories.Length} files.", silent);

			_fileCount = 1;

			_lib.WriteMessage("\n", silent);

			AnsiConsole.Progress()
				.AutoClear(false)

				.Columns(new ProgressColumn[]
					{
						new TaskDescriptionColumn(),    // Task description
                        new ProgressBarColumn(),        // Progress bar
                        new PercentageColumn(),         // Percentage
                        new SpinnerColumn(),            // Spinner
                    })

				.Start(ctx =>
				{
					var task = ctx.AddTask("[green]Processing files...[/]");
					task.MaxValue(directories.Length);

					foreach (string dirString in directories)
					{
						//_lib.WriteMessage($"File {dirString} processed.", silent);
						task.Increment(1);

						if (dirString != null && dirString.Length == 0 && dirString != string.Empty && dirString != "")
						{
							_lib.WriteMessage("Skipping empty file path.", "red", silent);
							continue;
						}

						if (!File.Exists(dirString))
						{
							_lib.WriteMessage($"File does not exist: {dirString}", "red", silent);
							continue;
						}

						FileInfo fileInfo = new FileInfo(dirString);
						ICollection<KeyValuePair<string, string>> exifData = exifTool.ExtractAllMetadata(dirString);
						Dictionary<string, string> exifDataDictionary = exifData.ToDictionary();

						if (!_ruleFile.rule.metaInfoField.ContainsKey(fileInfo.Extension.ToUpperInvariant()))
						{
							if (!NoMetadataRule.Contains(fileInfo.Extension.ToUpperInvariant()))
								NoMetadataRule.Add(fileInfo.Extension.ToUpperInvariant());
							continue;
						}

						MetaInfoField metaInfoField = _ruleFile.rule.metaInfoField[fileInfo.Extension.ToUpperInvariant()];

						// All schlüsselwürter für diesen Dateityp durchgehen und
						// die werte holen und in die datenbank schreiben
						foreach (var field in metaInfoField.availableKeyWords)
						{
							if (lastField != field.Key)
							{
								lastField = field.Key;
								currentDateTimeValue = fileInfo.CreationTime; // Reset currentDateTimeValue for each new field
							}


							foreach (var keyword in field.Value.KeyWords)
							{
								if (field.Value.Source == "EXIFTOOL")
								{
									if (!exifDataDictionary.TryGetValue(keyword, out value))
									{
										//_lib.WriteMessage($"Keyword '{keyword}' not found in EXIF data for file {fileInfo.Name}.", silent);
										continue;
									}
								}
								else if (field.Value.Source == "FILEINFO")
								{
									if (!fileInfo.GetType().GetProperty(keyword)?.CanRead ?? true)
									{
										//_lib.WriteMessage($"Keyword '{keyword}' not found in FileInfo for file {fileInfo.Name}.", silent);
										continue;
									}
									value = fileInfo.GetType().GetProperty(keyword)?.GetValue(fileInfo)?.ToString() ?? string.Empty;
								}
								else
								{
									//_lib.WriteMessage($"Unknown source '{field.Value.Source}' for keyword '{keyword}'.", silent);
									continue;
								}


								switch (field.Value.DataType.ToUpperInvariant())
								{
									case "STRING":
										propertyInfo = fileMetaInfo.GetType().GetProperty(field.Key);
										propertyInfo.SetValue(fileMetaInfo, value, null);
										break;

									case "INT":
										if (int.TryParse(value, out int intValue))
										{
											propertyInfo = fileMetaInfo.GetType().GetProperty(field.Key);
											propertyInfo.SetValue(fileMetaInfo, intValue, null);
										}
										break;

									case "LONG":
										if (long.TryParse(value, out long longValue))
										{
											propertyInfo = fileMetaInfo.GetType().GetProperty(field.Key);
											propertyInfo.SetValue(fileMetaInfo, longValue, null);
										}
										break;

									case "DOUBLE":
										if (double.TryParse(value, out double doubleValue))
										{
											propertyInfo = fileMetaInfo.GetType().GetProperty(field.Key);
											propertyInfo.SetValue(fileMetaInfo, doubleValue, null);
										}
										break;

									case "BOOLEAN":
										if (bool.TryParse(value, out bool boolValue))
										{
											propertyInfo = fileMetaInfo.GetType().GetProperty(field.Key);
											propertyInfo.SetValue(fileMetaInfo, boolValue, null);
										}
										break;

									case "DATETIME":
										CultureInfo enUS = new CultureInfo("en-US");
										DateTime dateTimeValue;

										if (
											(DateTime.TryParseExact(value, "dd.MM.yyyy HH:mm:ss", new CultureInfo("de-DE"), DateTimeStyles.None, out dateTimeValue)) ||
											(DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", null, DateTimeStyles.None, out dateTimeValue)) ||
											(DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:sszzz", null, DateTimeStyles.None, out dateTimeValue)) ||
											(DateTime.TryParseExact(value, "yyyy:MM:dd", null, DateTimeStyles.None, out dateTimeValue))
											)
										{
											if (field.Value.ValAttribute == "LOWEST" && dateTimeValue < currentDateTimeValue)
												currentDateTimeValue = dateTimeValue;
											else if (field.Value.ValAttribute == "HIGHEST" && dateTimeValue > currentDateTimeValue)
												currentDateTimeValue = dateTimeValue;
											else
												currentDateTimeValue = dateTimeValue;

											propertyInfo = fileMetaInfo.GetType().GetProperty(field.Key);
											propertyInfo.SetValue(fileMetaInfo, currentDateTimeValue, null);
										}
										break;

									default:
										_lib.WriteMessage($"Unknown data type: {field.Value.DataType}", "red", silent);
										break;
								}

							}
						}

						PaternInterpreter paternInterpreter = new PaternInterpreter();

						// Die Regeln für diesen Dateityp holen
						FileRules fileRules = _ruleFile.rule.fileRules[fileInfo.Extension.ToUpperInvariant()];

						string sortingPath = paternInterpreter.PaternParser(paternInterpreter.PaternLexer(fileRules.SortingPatern), fileMetaInfo);
						string newFileName = paternInterpreter.PaternParser(paternInterpreter.PaternLexer(fileRules.FileNamePatern), fileMetaInfo);

						if (fileRules.RootPath != null && fileRules.RootPath != string.Empty)
							if (fileRules.RootPath == "{MediaRootPath}")
								fileRules.RootPath = _ruleFile.rule.MediaRootPath;

						if (fileRules.RootPath.EndsWith("\\") || fileRules.RootPath.EndsWith("/"))
							fileMetaInfo.NewFilePath = Path.GetFullPath(fileRules.RootPath + sortingPath + "\\" + newFileName);
						else
							fileMetaInfo.NewFilePath = Path.GetFullPath(fileRules.RootPath + "\\" + sortingPath + "\\" + newFileName);

						fileMetaInfo.Id = _fileCount;
						_database.AddFileMetaInfo(fileMetaInfo, _dataBaseFileName);
						_lib.WriteMessage($"{_fileCount} - File {fileMetaInfo.FileName} processed. Size: {fileMetaInfo.FileSize}", "aqua", silent);
						_fileCount++;
					}
				});

			_lib.WriteMessage("\n======================================", "gold3", false);
			_lib.WriteMessage($"{_fileCount - 1} files processed.", false);
			_lib.WriteMessage($"\nNo metadata rules defined for ", silent);
			foreach (var ext in NoMetadataRule)
			{
				_lib.WriteMessage($" {ext} ", "yellow", silent);
			}
		}

		public void ExecuteCopy(string v1, string v2)
		{
			throw new NotImplementedException();
		}

		public void ExecuteMove(string v1, string v2)
		{
			throw new NotImplementedException();
		}
	}
}