using SharpExifTool;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FSWApi
{
	public class Wizard
	{
		private RuleFile? _ruleFile;
		private Lib? _lib;
		private DataBase? _database;

		public event EventHandler GetherInformationsStarted;
		public class GetherInformationsStartedEventArgs : EventArgs
		{
			public string RuleFileName { get; set; }
			public string InputDirectory { get; set; }
			public int DirectorysLength { get; set; }
		}

		public event EventHandler GetherInformationsFinished;
		public class GetherInformationsFinishedEventArgs : EventArgs
		{
			public int FileCount { get; set; }
			public List<string> NoMetadataRule { get; set; }
		}

		public event EventHandler SkippingEmptyFilePath;
		public event EventHandler FileNotExists;

		public event EventHandler FileProcessed;
		public class FileProcessedEventArgs : EventArgs
		{
			public int FileCount { get; set; }
			public string FileName { get; set; }
			public long FileSize { get; set; }
		}

		public event EventHandler UnknownDataType;
		public class UnknownDataTypeEventArgs : EventArgs
		{
			public string DataType { get; set; }
		}

		public event EventHandler DataBaseCreated;
		public class DataBaseCreatedEventArgs : EventArgs
		{
			public string DataBaseFileName { get; set; }
		}


		public string RuleFileName { get; set; }

		private string DataBaseFileName { get; set; }

		private void InitializeComponents(string ruleFileName)
		{
			// <exception cref="ArgumentNullException">Thrown when <paramref name="ruleFileName"/> is null.</exception>
			if (ruleFileName == null)
			{
				throw new ArgumentNullException(nameof(ruleFileName));
			}

			_ruleFile = new RuleFile();
			_lib = new Lib();

			// Deserialize rule file
			_ruleFile.Deserialize(ruleFileName);

			// Initialize log file
			_lib.ResetLog(_ruleFile.rule.logFileName);

			// Initialize database
			DataBaseFileName = Path.GetFullPath(_ruleFile.rule.DataBasePath + "\\" + _ruleFile.rule.DataBaseFileName);
			OnDataBaseCreated(new DataBaseCreatedEventArgs() { DataBaseFileName = DataBaseFileName });
			_database = new DataBase(DataBaseFileName);
		}

		public Wizard()
		{
		}

		public void GetherInformations(string ruleFileName, string? inputDirectory)
		{
			ExifTool exifTool = new SharpExifTool.ExifTool();
			List<string> NoMetadataRule = new List<string>();
			DateTime currentDateTimeValue = DateTime.MinValue;
			PropertyInfo propertyInfo;
			FileMetaInfo fileMetaInfo = new FileMetaInfo();

			string lastField = "";
			var value = "";

			InitializeComponents(ruleFileName);

			int _fileCount = 1;

			if (ruleFileName == null) throw new ArgumentNullException(nameof(ruleFileName));
			RuleFileName = ruleFileName;

			// Deserialize rule file
			_ruleFile.Deserialize(RuleFileName);

			// Initialize log file
			_lib.ResetLog(_ruleFile.rule.logFileName);

			string[] directories = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
			Array.Sort(directories);

			OnGetherInformationsStarted(new GetherInformationsStartedEventArgs() { RuleFileName = ruleFileName, InputDirectory = inputDirectory, DirectorysLength = directories.Length });

			_fileCount = 1;

			foreach (string dirString in directories)
			{
				if (dirString != null && dirString.Length == 0 && dirString != string.Empty && dirString != "")
				{
					OnSkippingEmptyFilePath();
					continue;
				}

				if (!File.Exists(dirString))
				{
					OnFileNotExists();
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
								OnUnknownDataType(new UnknownDataTypeEventArgs() { DataType = field.Value.DataType });
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
				_database.AddFileMetaInfo(fileMetaInfo, DataBaseFileName);
				OnFileProcessed(new FileProcessedEventArgs() { FileCount = _fileCount, FileName = fileInfo.Name, FileSize = fileInfo.Length });
				_fileCount++;
			}

			GetherInformationsFinishedEventArgs informationsFinishedEventArgs = new GetherInformationsFinishedEventArgs();
			informationsFinishedEventArgs.FileCount = _fileCount;
			informationsFinishedEventArgs.NoMetadataRule = NoMetadataRule;

			OnGetherInformationsFinished(informationsFinishedEventArgs);
		}

		protected virtual void OnGetherInformationsStarted(GetherInformationsStartedEventArgs e)
		{
			GetherInformationsStarted?.Invoke(this, e);
		}
		protected virtual void OnGetherInformationsFinished(GetherInformationsFinishedEventArgs e)
		{
			GetherInformationsFinished?.Invoke(this, e);
		}
		protected virtual void OnSkippingEmptyFilePath()
		{
			SkippingEmptyFilePath?.Invoke(this, EventArgs.Empty);
		}
		protected virtual void OnFileNotExists()
		{
			FileNotExists?.Invoke(this, EventArgs.Empty);
		}
		protected virtual void OnFileProcessed(FileProcessedEventArgs e)
		{
			FileProcessed?.Invoke(this, e);
		}
		protected virtual void OnUnknownDataType(UnknownDataTypeEventArgs e)
		{
			UnknownDataType?.Invoke(this, e);
		}
		protected virtual void OnDataBaseCreated(DataBaseCreatedEventArgs e)
		{
			DataBaseCreated?.Invoke(this, e);
		}
	}
}
