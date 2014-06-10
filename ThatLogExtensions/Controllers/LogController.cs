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
            LogBrowsers.Add("filesystem", new FileSystemLogBrowser("File System", "c:\\temp"));
            // LogBrowsers.Add("filesystem", new FileSystemLogBrowser(Environment.ExpandEnvironmentVariables("%HOME%") + "\\LogFiles"));

            var sasUrl = ConfigurationManager.AppSettings["DIAGNOSTICS_AZUREBLOBCONTAINERSASURL"];
            if (sasUrl != null)
            {
                LogBrowsers.Add("blobapplication", new StorageLogBrowser("Application Logs - Blob Storage", sasUrl));
            }

            sasUrl = ConfigurationManager.AppSettings["WEBSITE_HTTPLOGGING_CONTAINER_URL"];
            if (sasUrl != null)
            {
                LogBrowsers.Add("blobhttp", new StorageLogBrowser("HTTP Logs - Blob Storage", sasUrl));
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
        public HttpResponseMessage Download(string path, bool download)
        {
            path = Path.Combine("c:\\", path).Replace('/', '\\');
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