using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterWebMVCv2.Models;

namespace TwitterWebMVCv2.CountObjects
{
    public class HashtagIdCount
    {
        public int HashtagId { get; set; }
        public int TimesUsed { get; set; }
        public List<TweetHashtag> TweetHashtags { get; set; }
    }
}
