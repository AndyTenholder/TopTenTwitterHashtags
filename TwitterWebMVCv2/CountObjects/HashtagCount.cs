using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterWebMVCv2.Models;

namespace TwitterWebMVCv2.CountObjects
{
    public class HashtagCount
    {
        public Hashtag Hashtag { get; set; }
        public int TimesUsed { get; set; }
        public int[] TweetsPer { get; set; }

    }
}
