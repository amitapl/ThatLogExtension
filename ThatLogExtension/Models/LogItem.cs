namespace ThatLogExtensions.Models
{
    public class LogItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public long Size { get; set; }

        public bool IsDirectory { get; set; }

        public string Url { get; set; }

        public string DownloadUrl { get; set; }
    }
}
