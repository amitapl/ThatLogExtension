using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace ThatLogExtension.Models
{
    public class FileSystemLogBrowser : LogBrowser
    {
        private readonly FileSystem _fileSystem;

        public FileSystemLogBrowser(string name, string rootPath)
            : base(name)
        {
            _fileSystem = new FileSystem();
            RootPath = rootPath;
        }

        public string RootPath { get; private set; }

        public override LogItem GetLogItem(string baseAddress, string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                path = ".";
            }
            path = path.TrimStart('/');

            string windowsPath = Path.GetFullPath(Path.Combine(RootPath, path.Replace('/', '\\')));

            if (_fileSystem.Directory.Exists(windowsPath))
            {
                var logItems = new List<LogItem>();
                var logDirectory = new LogItems()
                {
                    Name = Path.GetFileName(windowsPath),
                    Path = windowsPath,
                    Url = baseAddress,
                    Root = Name,
                    Items = logItems
                };

                var directory = _fileSystem.DirectoryInfo.FromDirectoryName(windowsPath);
                var files = directory.GetFileSystemInfos();
                foreach (var file in files)
                {
                    var logItem = new LogItem()
                    {
                        Path = Path.Combine(windowsPath, file.Name),
                        IsDirectory = file.Attributes.HasFlag(System.IO.FileAttributes.Directory),
                        Name = file.Name,
                        Url = baseAddress + "/" + file.Name,
                        DownloadUrl = baseAddress + "/" + file.Name + "&download=true"
                    };

                    var fileInfoBase = file as FileInfoBase;
                    if (fileInfoBase != null)
                    {
                        logItem.Size = fileInfoBase.Length;
                    }

                    logItems.Add(logItem);
                }

                return logDirectory;
            }

            var fileInfo = _fileSystem.FileInfo.FromFileName(windowsPath);
            if (fileInfo.Exists)
            {
                return new LogItem()
                {
                    Path = windowsPath,
                    IsDirectory = false,
                    Name = Path.GetFileName(windowsPath),
                    Size = fileInfo.Length,
                    Url = baseAddress,
                    DownloadUrl = baseAddress + "&download=true"
                };
            }

            return null;
        }

        public override async Task<Stream> GetStreamForDownloadAsync(string path)
        {
            path = Path.Combine(RootPath, path.Trim('/')).Replace('/', '\\');
            return new FileStream(path, FileMode.Open);
        }
    }
}
