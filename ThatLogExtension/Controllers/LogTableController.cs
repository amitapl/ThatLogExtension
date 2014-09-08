using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Data.OData;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using Newtonsoft.Json;
using ThatLogExtension.Models;

namespace ThatLogExtension.Controllers
{
    public class LogTableController : ApiController
    {
        private const int MaxResults = 1000;

        public HttpResponseMessage Get(DateTime from, DateTime to, string token = null)
        {
            string sasUrl = ConfigurationManager.AppSettings["DIAGNOSTICS_AZURETABLESASURL"];
            if (String.IsNullOrEmpty(sasUrl))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
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

                    var table = tableClient.GetTableReference(tableName);
                    var tableQuery =
                        new TableQuery<EventEntity>()
                        {
                            TakeCount = MaxResults,
                            FilterString = TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, GetDatePartitionKey(from)),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, GetDatePartitionKey(to)))
                        };

                    TableQuerySegment<EventEntity> results;
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
                        Token = results.Results.Count >= MaxResults ? JsonConvert.SerializeObject(results.ContinuationToken) : null
                    });
                }
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        private static string GetDatePartitionKey(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyyMMddHH");
        }
    }
}
