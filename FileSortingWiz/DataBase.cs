using Microsoft.EntityFrameworkCore;

namespace FileSortingWiz
{
    internal class FileMetaInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? NewFilePath { get; set; }
        public string? Attributes { get; set; }
        public long? FileSize { get; set; }
        public DateTime? CreationTime { get; set; } = DateTime.MinValue;
        public DateTime? ModificationTime { get; set; } = DateTime.MinValue;
        public DateTime? AccessTime { get; set; } = DateTime.MinValue;
        public bool? IsReadOnly { get; set; } = false;
        public string? MIMEType { get; set; }
        public string? Permissions { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Orientation { get; set; } = 0;
        public int? XResolution { get; set; } = 0;
        public int? YResolution { get; set; } = 0;
        public string? ResolutionUnit { get; set; } = string.Empty;
        public int? ImageWidth { get; set; } = 0;
        public int? ImageHeight { get; set; } = 0;
        public DateTime? MetaCreationTime { get; set; } = DateTime.MinValue;
        public DateTime? MetaModificationTime { get; set; } = DateTime.MinValue;
        public DateTime? MetaAccessTime { get; set; } = DateTime.MinValue;
        public DateTime? MetaCreationTimeUtc { get; set; } = DateTime.MinValue;
        public string? Compression { get; set; } = string.Empty;
        public double? ApertureValue { get; set; } = 0.0;
        public double? MaxApertureValue { get; set; } = 0.0;
        public string? FileSource { get; set; } = string.Empty;
        public int? FocalLengthIn35mmFormat { get; set; } = 0;
        public string? SerialNumber { get; set; } = string.Empty;
        public string? LensInfo { get; set; } = string.Empty;
        public string? LensModel { get; set; } = string.Empty;
        public string? ExposureTime { get; set; } = string.Empty;
        public string? LightSource { get; set; } = string.Empty;
        public string? Flash { get; set; } = string.Empty;
        public string? ShutterSpeed { get; set; } = string.Empty;
    }

    internal class TSFileWizContext : DbContext
    {
        private string _dataBaseFileName;
        private DbSet<FileMetaInfo> _fileMetaInfos;

        public TSFileWizContext(string dataBaseFileName)
        {
            _dataBaseFileName = dataBaseFileName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbConnectionString = $"Data Source={_dataBaseFileName};";
                optionsBuilder.UseSqlite(dbConnectionString);
                base.OnConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<FileMetaInfo>().ToTable("FileMetaInfos");
            modelBuilder.Entity<FileMetaInfo>().HasKey(f => f.Id);
            modelBuilder.Entity<FileMetaInfo>().Property(f => f.Id).ValueGeneratedOnAdd();
        }
    }

    internal class DataBase
    {
        internal void InitializeDataBase(string dataBaseFileName)
        {
            if(File.Exists(dataBaseFileName))
            {
                File.Delete(dataBaseFileName);
            }

            DbContext dbContext = new TSFileWizContext(dataBaseFileName);
            dbContext.Database.EnsureCreated();

            //dbContext.RemoveRange(dbContext.Set<FileMetaInfo>());
        }

        internal void AddFileMetaInfo(FileMetaInfo fileMetaInfo, string dataBaseFileName)
        {
            using (var dbContext = new TSFileWizContext(dataBaseFileName))
            {
                dbContext.Set<FileMetaInfo>().Add(fileMetaInfo);
                dbContext.SaveChanges();
            }
        }
    }
}
