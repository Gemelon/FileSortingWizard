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

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


namespace FSWApi
{
    /// <summary>
    /// Represents a set of rules and configurations for managing file paths, metadata, and file-specific rules.
    /// </summary>
    /// <remarks>This class provides properties to define paths for various media types, a database
    /// configuration,  and dictionaries for metadata fields and file-specific rules. It is designed to centralize and
    /// organize  file management settings.</remarks>
    public class Rule
    {
        public string DataBasePath { get; set; }
        public string DataBaseFileName { get; set; }
        public string logFileName { get; set; }
		public string MediaRootPath { get; set; }
		public string videoRootPath { get; set; }
		public string imageRootPath { get; set; }
		public string documentRootPath { get; set; }
		public string musicRootPath { get; set; }
		
		public Dictionary<String, MetaInfoField> metaInfoField { get; set; }
        public Dictionary<string, FileRules> fileRules { get; set; }
    }

    public class MetaInfoField
    {
        //public string Identifier { get; set; }
        public Dictionary<String, AvailableKeyWords> availableKeyWords { get; set; }
    }

    public class AvailableKeyWords
    {
        public string DataType { get; set; }
        public string Source { get; set; }
        public string ValAttribute { get; set; }
        public string[] KeyWords { get; set; }
    }

    public class FileRules
    {
        public string Category { get; set; }
        public string RootPath { get; set; }
        public string SortingPatern { get; set; }
        public string FileNamePatern { get; set; }
    }

    /// <summary>
    /// Represents a file containing rules for organizing and managing files.
    /// </summary>
    /// <remarks>The <see cref="RuleFile"/> class provides functionality to deserialize rules from a YAML file
    /// and serialize rules for testing purposes. It encapsulates a <see cref="Rule"/> object that defines the structure
    /// and behavior of the rules.</remarks>
    public class RuleFile
    {
        public Rule rule { get; set; }

        public void Deserialize(string ruleFileName)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();

            var yml = File.ReadAllText(ruleFileName);
            rule = deserializer.Deserialize<Rule>(yml);
        }

		/// <summary>
		/// For testing purposes only
		/// </summary>
		/// <param name="ruleFileName"></param>
		public void Serialize(string ruleFileName)
        {
            var rule = new Rule
            {
                DataBasePath = "C:/FileSortingWiz/Database",
                DataBaseFileName = "file_database.db",
                MediaRootPath = "D:/Media",
                metaInfoField = new Dictionary<string, MetaInfoField>
                {
                    {
                        "Title", new MetaInfoField
                        {
                            availableKeyWords = new Dictionary<string, AvailableKeyWords>
                            {
                                {
                                    "Default", new AvailableKeyWords
                                    {
                                        DataType = "String",
                                        Source = "Metadata",
                                        ValAttribute = "title",
                                        KeyWords = new string[] { "Inception", "The Matrix", "Interstellar" }
                                    }
                                }
                            }
                        }
                    }
                },
                fileRules = new Dictionary<string, FileRules>
                {
                    {
                        "Movies", new FileRules
                        {
                            Category = "Movies",
                            RootPath = "D:/Media/Movies",
                            SortingPatern = "{Year}/{Genre}/",
                            FileNamePatern = "{Title} ({Year})"
                        }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(rule);
            System.Console.WriteLine(yaml);
        }
    }
}
