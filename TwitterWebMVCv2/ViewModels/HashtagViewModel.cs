using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterWebMVCv2.CountObjects;

namespace TwitterWebMVCv2.ViewModels
{
    public class HashtagViewModel
    {
        public string HashtagName { get; set; }
        public IList<LanguageCount> LanguageCounts { get; set; }
        public IList<HashtagCount> HashtagCounts { get; set; }
        public int[] TweetsPerHour { get; set; }

        public int TotalTweets { get; set; }
        public int TotalLanguages { get; set; }
        public int TotalHashtags { get; set; }

        public HashtagViewModel(IList<LanguageCount> languageCounts, IList<HashtagCount> hashtagCounts, int[] tweetsPerHour,
             int totalTweets, int totalLanguages, int totalHashtags, string hashtagName)
        {
            LanguageCounts = languageCounts;
            HashtagCounts = hashtagCounts;
            TweetsPerHour = tweetsPerHour;
            TotalTweets = totalTweets;
            TotalLanguages = totalLanguages;
            TotalHashtags = totalHashtags;
            HashtagName = hashtagName;
        }
    }
}
