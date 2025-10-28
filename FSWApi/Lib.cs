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

using Microsoft.VisualBasic.FileIO;
using Spectre.Console;
using System.Text;

namespace FSWApi
{
	/// <summary>
	/// Provides utility methods for writing messages to a log file, optionally displaying them in the console, and
	/// performing string manipulations.
	/// </summary>
	/// <remarks>This class includes methods for logging messages with optional console output, resetting the
	/// log file, and replacing characters in a string with a specified maximum replacement count.</remarks>
	public class Lib
	{
		private string _logFileName = null;

		public string LogFileName
		{
			get { return _logFileName; }
			set { _logFileName = value; }
		}

		private string _logFilePath = null;

		public string LogFilePath
		{
			get { return _logFilePath; }
			set { _logFilePath = value; }
		}

		/// <summary>
		/// Writes a message to the console with the specified color and logs it to a file.
		/// </summary>
		/// <remarks>The method writes the message to a log file located in the "TSFileWizard" directory
		/// under the user's "Documents" folder.  If the directory does not exist, it will be created. The log file is
		/// named "TSFileWizard.log" and messages are appended to it.</remarks>
		/// <param name="message">The message to be written and logged. Cannot be null or empty.</param>
		/// <param name="color">The color to use when displaying the message in the console. Must be a valid ANSI color code.</param>
		/// <param name="silent">A boolean value indicating whether the message should be displayed in the console.  If <see
		/// langword="true"/>, the message will only be logged to the file; otherwise, it will be displayed and logged.</param>
		public void WriteMessage(string message, string color, bool? silent)
		{
			if (LogFileName == null)
			{
				throw new InvalidOperationException("Log file name is not set. Please set the log file name before writing messages.");
			}

			if (LogFilePath == null)
			{
				throw new InvalidOperationException("Log file path is not set. Please set the log file path before writing messages.");
			}

			if (silent == false)
			{
				AnsiConsole.MarkupLine($"[{color}]{message}[/]");
			}

			if (!Directory.Exists(LogFilePath))
				Directory.CreateDirectory(LogFilePath);

			using (TextWriter writer = new StreamWriter(LogFilePath + "\\" + LogFileName, true, Encoding.UTF8))
			{
				writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
			}
		}

		/// <summary>
		/// Writes a message to the output with the specified text and an optional silent mode.
		/// </summary>
		/// <param name="message">The message text to be written. Cannot be null or empty.</param>
		/// <param name="silent">A nullable boolean indicating whether the message should be written silently. If <see langword="true"/>, the
		/// message is written in silent mode; if <see langword="false"/>, it is written normally. If <see
		/// langword="null"/>, the default behavior is applied.</param>
		public void WriteMessage(string message, bool? silent)
		{
			WriteMessage(message, "green", silent);
		}

		/// <summary>
		/// Deletes the log file for the application if it exists.
		/// </summary>
		/// <remarks>This method removes the log file located in the user's "My Documents" directory. If the file does
		/// not exist, no action is taken.</remarks>
		public void ResetLog(string logFileName)
		{
			LogFileName = logFileName;
			LogFilePath = SpecialDirectories.MyDocuments + "\\FileSortingWizard";

			if (!Directory.Exists(LogFilePath))
				Directory.CreateDirectory(LogFilePath);

			if (File.Exists(LogFilePath + "\\" + LogFileName))
			{
				File.Delete(LogFilePath + "\\" + LogFileName);
			}
		}

		/// <summary>
		/// Resets the log file to its default state.
		/// </summary>
		/// <remarks>This method clears the contents of the log file and resets it to the default file name,
		/// "FileSortingWizard.log". Use this method to ensure the log file starts fresh with no previous entries.</remarks>
		public void ResetLog()
		{
			ResetLog("FileSortingWizard.log");
		}

		/// <summary>
		/// Replaces occurrences of a specified character in a string with another character, up to a maximum number of
		/// replacements.
		/// </summary>
		/// <remarks>This method processes the input string character by character and performs replacements until the
		/// specified maximum count is reached. If <paramref name="MaxCount"/> exceeds the number of occurrences of <paramref
		/// name="oldChar"/>, all occurrences are replaced.</remarks>
		/// <param name="input">The input string in which to perform the replacements. Cannot be <see langword="null"/>.</param>
		/// <param name="oldChar">The character to be replaced.</param>
		/// <param name="newChar">The character to replace <paramref name="oldChar"/> with.</param>
		/// <param name="MaxCount">The maximum number of replacements to perform. Must be greater than or equal to 0.</param>
		/// <returns>A new string with up to <paramref name="MaxCount"/> occurrences of <paramref name="oldChar"/> replaced by
		/// <paramref name="newChar"/>. If <paramref name="MaxCount"/> is 0, the original string is returned unchanged.</returns>
		public string StringReplace(string input, char oldChar, char newChar, int MaxCount)
		{
			string newString = "";
			int charCount = 0;

			foreach (char c in input)
			{
				if (c == oldChar && charCount < MaxCount)
				{
					newString += newChar;
					charCount++;
				}
				else
				{
					newString += c;
				}
			}

			return newString;
		}
	}
}
