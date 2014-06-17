using System;
using System.IO;
using System.Threading.Tasks;

namespace ThatLogExtension.Models
{
    public class TableStorageLogBrowser : LogBrowser
    {
        public TableStorageLogBrowser(string name)
            : base(name)
        {
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
            return "viewtable.cshtml";
        }
    }
}
