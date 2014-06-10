using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ThatLogExtensions.Models
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
                            Url = baseAddress + innerName,
                            DownloadUrl = blob.Uri + _sas
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
                    Url = baseAddress,
                    DownloadUrl = blockBlobReference.Uri + _sas
                };
            }

            return null;
        }
    }
}