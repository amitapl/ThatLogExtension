using System.IO;
using System.Threading.Tasks;

namespace ThatLogExtension.Models
{
    public interface ILogBrowser
    {
        LogItem GetLogItem(string baseAddress, string path);

        Task<Stream> GetStreamForDownloadAsync(string path);

        string Name { get; }

        string BuildExternalUrl();
    }
}
