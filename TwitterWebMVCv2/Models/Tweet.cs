using System;
using System.Collections.Generic;

namespace TwitterWebMVCv2.Models
{
    public class Tweet
    {
        public int ID { get; set; }
        public string TweetIdString { get; set; }
        public int UnixTimeStamp { get; set; }

        // foreign key
        public int LanguageID { get; set; }
        // Navigation prop
        public Language Language { get; set; }

        public String Hashtags { get; set; }
    }
}