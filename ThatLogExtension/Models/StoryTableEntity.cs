using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace ThatLogExtension.Models
{
    public class StoryTableEntity : TableEntity
    {
        public string Name { get; set; }

        public string InstanceId { get; set; }

        public DateTime StartDateTime { get; set; }

        public TimeSpan Elapsed { get; set; }

        public string Json { get; set; }
    }
}
