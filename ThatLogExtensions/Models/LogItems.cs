using System.Collections.Generic;

namespace ThatLogExtensions.Models
{
    public class LogItems : LogItem
    {
        public string Root { get; set; }

        public IEnumerable<LogItem> Items { get; set; }
    }
}
