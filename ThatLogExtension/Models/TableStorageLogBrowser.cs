using System;
using System.IO;
using System.Threading.Tasks;

namespace ThatLogExtension.Models
{
    public class TableStorageLogBrowser : LogBrowser
    {
        private readonly string _externalUrl;

        public TableStorageLogBrowser(string name, string externalUrl)
            : base(name)
        {
            _externalUrl = externalUrl;
        }

        public override LogItem GetLogItem(string baseAddress, string path)
        {
            throw new InvalidOperationException();
        }

        public override Task<Stream> GetStreamForDownloadAsync(string path)
        {
            throw new InvalidOperationException();
        }

        public override string BuildExternalUrl()
        {
            return _externalUrl;
        }
    }
}
