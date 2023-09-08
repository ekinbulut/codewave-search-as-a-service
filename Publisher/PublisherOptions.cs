using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher
{
    public class PublisherOptions
    {
        public int BatchSize { get; set; }
        public string Exchange { get; set; }
        public string Queue { get; set; }
    }
}
