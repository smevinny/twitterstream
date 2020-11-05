using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterStream.Models
{
    public class TwitterData
    {
        public int TotalTweets { get; set; }
        public int AverageTweetPerHour { get; set; }
        public int AverageTweetPerMinute { get; set; }
        public int AverageTweetPerSecond { get; set; }
        public List<string> TopEmojis { get; set; }
        public int TotalTweetWithEmoji { get; set; }
        public decimal EmojiPercentage { get; set; }
        public List<string> TopHashtags { get; set; }
        public int TotalTweetWithUrl { get; set; }
        public decimal UrlPercentage { get; set; }
        public int TotalTweetWithPhoto { get; set; }
        public decimal PhotoPercentage { get; set; }
        public List<string> TopDomains { get; set; }
    }
}
