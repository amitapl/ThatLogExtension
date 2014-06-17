using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ThatLogExtension.Models
{
    public class StorageLogBrowser : LogBrowser
    {
        private readonly string _sasUrl;
        private readonly string _sas;

        public StorageLogBrowser(string name, string sasUrl)
            : base(name)
        {
            _sasUrl = sasUrl;
            _sas = sasUrl.Substring(sasUrl.IndexOf("?", StringComparison.Ordinal));
        }

        public override LogItem GetLogItem(string baseAddress, string path)
        {
            if (_sasUrl == null)
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(path))
            {
                path = String.Empty;
            }

            path = path.TrimStart('/');

            var blobContainer = new CloudBlobContainer(new Uri(_sasUrl));

            var name = Path.GetFileName(path.TrimEnd('/').Replace("/", "\\"));

            if (path.EndsWith("/") || path == String.Empty)
            {
                var logItems = new List<LogItem>();
                var logDirectory = new LogItems()
                {
                    Url = baseAddress,
                    Name = name,
                    IsDirectory = true,
                    Path = path,
                    Root = Name,
                    Items = logItems
                };

                var blobs = blobContainer.ListBlobs(String.IsNullOrEmpty(path) ? null : path);
                foreach (var blob in blobs)
                {
                    if (blob is CloudBlockBlob)
                    {
                        var cloudBlockBlob = blob as CloudBlockBlob;
                        var innerName = Path.GetFileName(cloudBlockBlob.Name.TrimEnd('/').Replace("/", "\\"));
                        logItems.Add(new LogItem()
                        {
                            Name = innerName,
                            Size = cloudBlockBlob.Properties.Length,
                            Path = path + innerName,
                            Date = cloudBlockBlob.Properties.LastModified.HasValue ? (DateTime?)cloudBlockBlob.Properties.LastModified.Value.DateTime : null,
                            Url = baseAddress + innerName,
                            DownloadUrl = baseAddress + innerName + "&download=true"
                        });
                    }
                    else if (blob is CloudBlobDirectory)
                    {
                        var innerCloudBlobDirectory = blob as CloudBlobDirectory;
                        var innerPath = innerCloudBlobDirectory.Prefix;
                        var innerName = Path.GetFileName(innerPath.TrimEnd('/').Replace("/", "\\"));
                        logItems.Add(new LogItem()
                        {
                            IsDirectory = true,
                            Name = innerName,
                            Path = innerPath,
                            Url = baseAddress + innerName + "/"
                        });
                    }
                }

                return logDirectory;
            }

            var blockBlobReference = blobContainer.GetBlockBlobReference(path);
            if (blockBlobReference.Exists())
            {
                return new LogItem()
                {
                    Name = name,
                    Size = blockBlobReference.Properties.Length,
                    Path = path,
                    Date = blockBlobReference.Properties.LastModified.HasValue ? (DateTime?)blockBlobReference.Properties.LastModified.Value.DateTime : null,
                    Url = baseAddress,
                    DownloadUrl = blockBlobReference.Uri + _sas
                };
            }

            return null;
        }

        public override async Task<Stream> GetStreamForDownloadAsync(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                path = String.Empty;
            }

            path = path.TrimStart('/');

            var blobContainer = new CloudBlobContainer(new Uri(_sasUrl));
            var blockBlobReference = blobContainer.GetBlockBlobReference(path);

            var httpClient = new HttpClient();
            return await httpClient.GetStreamAsync(blockBlobReference.Uri + _sas);
        }
    }
}