namespace ThatLogExtensions.Models
{
    public interface ILogBrowser
    {
        LogItem GetLogItem(string baseAddress, string path);

        string Name { get; }
    }
}
