using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace MadeNLaidWeb.Models
{
    public class SISyndicationFeed : SyndicationFeed
    {
        public string title { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string copyright { get; set; }
        public string language { get; set; }
        public string managingEditor { get; set; }
        public DateTimeOffset pubDate { get; set; }
    }
}
