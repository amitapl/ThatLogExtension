using System.IO;
using System.Threading.Tasks;

namespace ThatLogExtension.Models
{
    public abstract class LogBrowser : ILogBrowser
    {
        protected LogBrowser(string name)
        {
            Name = name;
        }

        public abstract LogItem GetLogItem(string baseAddress, string path);

        public abstract Task<Stream> GetStreamForDownloadAsync(string path);

        public virtual string BuildExternalUrl()
        {
            return null;
        }

        public string Name { get; private set; }
    }
}
