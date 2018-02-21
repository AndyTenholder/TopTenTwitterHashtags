using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterWebMVCv2.ViewModels
{
    public class HashtagViewModel
    {
        public IList<Models.Language> Languages { get; set; }
        public IList<Models.Hashtag> Hashtags { get; set; }
        public int[] TweetsPerHour { get; set; }

        public int TotalTweets { get; set; }
        public int TotalLanguages { get; set; }
        public int TotalHashtags { get; set; }

        public HashtagViewModel(IList<Models.Language> languages, IList<Models.Hashtag> hashtags, int[] tweetsPerHour,
             int totalTweets, int totalLanguages, int totalHashtags)
        {
            Languages = languages;
            Hashtags = hashtags;
            TweetsPerHour = tweetsPerHour;
            TotalTweets = totalTweets;
            TotalLanguages = totalLanguages;
            TotalHashtags = totalHashtags;
        }
    }
}
