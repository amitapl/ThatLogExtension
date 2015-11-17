using Microsoft.Data.OData;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ThatLogExtension.Models;

namespace ThatLogExtension.Controllers
{
    [RoutePrefix("api")]
    public class LogTableController : ApiController
    {
        private const int TableMaxResults = 1000;
        private const int StoriesMaxResults = 100;

        [Route("logtable")]
        [HttpGet]
        public HttpResponseMessage GetTableData(DateTime from, DateTime to, string token = null)
        {
            var table = GetCloudTableFromSas(Utils.GetSetting("DIAGNOSTICS_AZURETABLESASURL"));

            return GetData<EventEntity>(from, to, token, table, TableMaxResults);
        }

        [Route("storytable")]
        [HttpGet]
        public HttpResponseMessage GetStoryData(DateTime from, DateTime to, string token = null)
        {
            var table = GetCloudTableFromConnectionString(Utils.GetSetting("StoryTableStorage"), "Stories");

            return GetData<StoryTableEntity>(from, to, token, table, StoriesMaxResults);
        }

        private HttpResponseMessage GetData<T>(DateTime from, DateTime to, string token, CloudTable table, int maxResults) where T : TableEntity, new()
        {
            if (table == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var a = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, GetDatePartitionKey(@from));
            var tableQuery =
                new TableQuery<T>()
                {
                    TakeCount = maxResults,
                    FilterString = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, GetDatePartitionKey(@from)),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, GetDatePartitionKey(to)))
                };

            TableQuerySegment<T> results;
            if (!String.IsNullOrEmpty(token))
            {
                results = table.ExecuteQuerySegmented(tableQuery, JsonConvert.DeserializeObject<TableContinuationToken>(token));
            }
            else
            {
                results = table.ExecuteQuerySegmented(tableQuery, null);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Items = results.Results,
                Token = results.Results.Count >= maxResults ? JsonConvert.SerializeObject(results.ContinuationToken) : null
            });
        }

        private CloudTable GetCloudTableFromSas(string sasUrl)
        {
            if (String.IsNullOrEmpty(sasUrl))
            {
                return null;
            }

            int parseIndex = sasUrl.IndexOf('?');
            if (parseIndex > 0)
            {
                string tableAddress = sasUrl.Substring(0, parseIndex);

                int tableParseIndex = tableAddress.LastIndexOf('/');
                if (tableParseIndex > 0)
                {
                    string tableName = tableAddress.Substring(tableParseIndex + 1);

                    string endpointAddress = tableAddress.Substring(0, tableParseIndex);
                    string sasSignature = sasUrl.Substring(parseIndex);

                    var tableClient = new CloudTableClient(new Uri(endpointAddress), new StorageCredentials(sasSignature));

                    if (sasSignature.IndexOf("sv=2012-02-12", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tableClient.DefaultRequestOptions.PayloadFormat = TablePayloadFormat.AtomPub;
                        var type = typeof(TableConstants);
                        var field = type.GetField("ODataProtocolVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        field.SetValue(null, ODataVersion.V2);
                    }

                    return tableClient.GetTableReference(tableName);
                }
            }

            return null;
        }

        private CloudTable GetCloudTableFromConnectionString(string connectionString, string tableName)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();

            return tableClient.GetTableReference(tableName);
        }

        private static string GetDatePartitionKey(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyyMMddHH");
        }
    }
}
