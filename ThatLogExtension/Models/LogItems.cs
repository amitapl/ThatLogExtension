using System.Collections.Generic;
using ThatLogExtensions.Models;

namespace ThatLogExtension.Models
{
    public class LogItems : LogItem
    {
        public string Root { get; set; }

        public IEnumerable<LogItem> Items { get; set; }
    }
}
