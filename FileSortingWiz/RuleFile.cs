using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FileSortingWiz
{
    public class Rule
    {
        public string DataBasePath { get; set; }
        public string DataBaseFileName { get; set; }
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

    //public class UpperCaseNamingPolicy : JsonNamingPolicy
    //{
    //    public override string ConvertName(string name) =>
    //        name.ToUpper();
    //}

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
