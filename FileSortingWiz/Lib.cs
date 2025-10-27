using Microsoft.VisualBasic.FileIO;
using Spectre.Console;
using System.Text;

namespace FileSortingWiz
{
    internal class Lib
    {
        public void WriteMessage(string message, string color, bool? silent)
        {
            if (silent == false)
            {
                //Console.WriteLine(message);
                AnsiConsole.MarkupLine($"[{color}]{message}[/]");
            }

            //string path = "D:\\Projekte\\Projekte_2025\\TSFileWizard\\TSFileWizard.log";
            if (!Directory.Exists(SpecialDirectories.MyDocuments + "\\TSFileWizard"))
                Directory.CreateDirectory(SpecialDirectories.MyDocuments + "\\TSFileWizard");
            string path = SpecialDirectories.MyDocuments + "\\TSFileWizard\\TSFileWizard.log";
            using (TextWriter writer = new StreamWriter(path, true, Encoding.UTF8))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }

        public void WriteMessage(string message, bool? silent)
        {
            WriteMessage(message, "green", silent);
        }


        public void ResetLog()
        {

            string path = "D:\\Projekte\\Projekte 2025\\TSFileWizard\\TSFileWizard.log";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

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
