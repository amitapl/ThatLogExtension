using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ThatLogExtensions.Models;

namespace ThatLogExtensions.Controllers
{
    public class LogController : ApiController
    {
        private static readonly Dictionary<string, ILogBrowser> LogBrowsers = new Dictionary<string, ILogBrowser>(StringComparer.InvariantCultureIgnoreCase);

        static LogController()
        {
            AddFileSystemLogBrowserBasedOnExistance(LogBrowsers, "c:\\temp", "filesystemtemp", "File System - Temp");
            AddFileSystemLogBrowserBasedOnExistance(LogBrowsers, "LogFiles\\Application", "filesystemapplication", "File System - Application Logs");
            AddFileSystemLogBrowserBasedOnExistance(LogBrowsers, "LogFiles\\http\\RawLogs", "filesystemhttp", "File System - HTTP Logs");
            AddFileSystemLogBrowserBasedOnExistance(LogBrowsers, "LogFiles\\DetailedErrors", "detailederrors", "IIS Detailed Errors");
            AddFileSystemLogBrowserBasedOnExistance(LogBrowsers, "LogFiles\\Git\\trace", "filesystemkudu", "File System - Kudu Logs");
            AddFileSystemLogBrowserBasedOnExistance(LogBrowsers, "LogFiles", "filesystem", "File System - Log Files Directory");

            AddStorageLogBrowserBasedOnEnvironment(LogBrowsers, "DIAGNOSTICS_AZUREBLOBCONTAINERSASURL", "blobapplication", "Application Logs - Blob Storage");
            AddStorageLogBrowserBasedOnEnvironment(LogBrowsers, "WEBSITE_HTTPLOGGING_CONTAINER_URL", "blobhttp", "HTTP Logs - Blob Storage");
        }

        private static void AddStorageLogBrowserBasedOnEnvironment(Dictionary<string, ILogBrowser> logBrowsers, string environmentVariableKey, string logBrowserKey, string logBrowserName)
        {
            var sasUrl = ConfigurationManager.AppSettings[environmentVariableKey];
            if (sasUrl != null)
            {
                logBrowsers.Add(logBrowserKey, new StorageLogBrowser(logBrowserName, sasUrl));
            }
        }

        private static void AddFileSystemLogBrowserBasedOnExistance(Dictionary<string, ILogBrowser> logBrowsers, string fileSystemPath, string logBrowserKey, string logBrowserName)
        {
            fileSystemPath = Path.Combine(Environment.ExpandEnvironmentVariables("%HOME%"), fileSystemPath);

            if (Directory.Exists(fileSystemPath))
            {
                logBrowsers.Add(logBrowserKey, new FileSystemLogBrowser(logBrowserName, fileSystemPath));
            }
        }

        public HttpResponseMessage Get(string type, string path)
        {
            if (String.IsNullOrEmpty(type))
            {
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new LogItems()
                    {
                        IsDirectory = true,
                        Items = LogBrowsers.Select(keyValuePair => new LogItem()
                        {
                            Name = keyValuePair.Value.Name,
                            IsDirectory = true,
                            Url = Request.RequestUri.AbsolutePath + "?type=" + keyValuePair.Key + "&path=/"
                        })
                    });
            }

            ILogBrowser logBrowser;
            if (!LogBrowsers.TryGetValue(type, out logBrowser))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, logBrowser.GetLogItem(Request.RequestUri.ToString(), path));
        }

        [HttpGet]
        public HttpResponseMessage Download(string type, string path, bool download)
        {
            ILogBrowser logBrowser;
            if (!LogBrowsers.TryGetValue(type, out logBrowser))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            path = Path.Combine(((FileSystemLogBrowser)logBrowser).RootPath, path.Trim('/')).Replace('/', '\\');
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(path);
            return result;
        }
    }
}