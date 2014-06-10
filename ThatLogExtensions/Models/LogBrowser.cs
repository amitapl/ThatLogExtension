namespace ThatLogExtensions.Models
{
    public abstract class LogBrowser : ILogBrowser
    {
        protected LogBrowser(string name)
        {
            Name = name;
        }

        public abstract LogItem GetLogItem(string baseAddress, string path);

        public string Name { get; private set; }
    }
}
