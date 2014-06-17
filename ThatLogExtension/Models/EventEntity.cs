using Microsoft.WindowsAzure.Storage.Table;

namespace ThatLogExtension.Models
{
    public class EventEntity : TableEntity
    {
        public long EventTickCount { get; set; }

        public string ApplicationName { get; set; }

        public string Level { get; set; }

        public int EventId { get; set; }

        public string InstanceId { get; set; }

        public int Pid { get; set; }

        public int Tid { get; set; }

        public string ActivityId { get; set; }

        public string Message { get; set; }
    }
}
