namespace FileSortingWiz
{
    public class Wizard
    {
        private Lib _lib;
        private RuleFile _ruleFile;

        TextReader _ruleFileReader;

        private void InitializeComponents(string ruleFileName)
        {
            _ruleFile = new RuleFile();
            _lib = new Lib();

            //_ruleFile.Serialize(ruleFileName);
            _ruleFile.Deserialize(ruleFileName);
            _lib.ResetLog();
        }

        public void CollectFileData(string ruleFileName, string inputDirectory, string outputDirectory, bool silent)
        {
            InitializeComponents(ruleFileName);
        }
    }
}