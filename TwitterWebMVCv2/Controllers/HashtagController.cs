using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TwitterWebMVCv2.Data;
using TwitterWebMVCv2.Models;
using TwitterWebMVCv2.CountObjects;
using TwitterWebMVCv2.Comparers;
using TwitterWebMVCv2.ViewModels;

namespace TwitterWebMVCv2.Controllers
{
    public class HashtagController : Controller
    {
        private TweetDbContext context;

        // All timestamps are in UTC time.  So UTC is used throughout the app

        DateTime dateTimeNow = DateTime.UtcNow;

        public HashtagController(TweetDbContext dbContext)
        {
            context = dbContext;
        }

        public IActionResult Index(int id)
        {
            Hashtag hashtag = context.Hashtags.Single(h => h.ID == id);
            List<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.HashtagID == id).ToList();
            List<TweetHashtag> tweethHashtagsPast24Hours = tweetHashtags.Where(th => UnixTimeStampToDateTime(th.UnixTimeStamp) > dateTimeNow.AddDays(-1)).ToList();
            List<Tweet> tweetsPast24Hours = context.Tweets.Where(t => UnixTimeStampToDateTime(t.UnixTimeStamp) > dateTimeNow.AddDays(-1)).ToList();
            List<Language> languages = context.Languages.ToList();

            List<Tweet> tweets = new List<Tweet>();

            foreach (TweetHashtag th in tweethHashtagsPast24Hours)
            {
                Tweet tweet = tweetsPast24Hours.Single(t => t.ID == th.TweetID);
                tweets.Add(tweet);
            }

            int totalTweets = tweets.Count * 100;

            List<int> languagesIdsUsed = new List<int>();
            List<LanguageCount> languageCounts = new List<LanguageCount>();

            List<String> hashtagStrings = new List<string>();
            List<HashtagStringCount> hashtagStringCounts = new List<HashtagStringCount>();

            // Create array to hold number of tweets in each hour
            int[] tweetsPerHour = new int[24];

            foreach (Tweet tweet in tweets)
            {
                // determine the hour the tweet was created and add 100 to the tweetsPerHour array
                int hour = UnixTimeStampToDateTime(tweet.UnixTimeStamp).Hour;
                tweetsPerHour[hour] += 100;

                // Retrieve the language of the tweet and keep track of the times that language has been used
                if (languagesIdsUsed != null && languagesIdsUsed.Contains(tweet.LanguageID))
                {
                    LanguageCount languageCount = languageCounts.Single(lc => lc.Language.ID == tweet.LanguageID);
                    languageCount.TimesUsed += 100;
                }
                else
                {
                    LanguageCount newLanguageCount = new LanguageCount
                    {
                        Language = languages.Single(l => l.ID == tweet.LanguageID),
                        TimesUsed = 100
                    };
                    languagesIdsUsed.Add(tweet.LanguageID);
                    languageCounts.Add(newLanguageCount);
                }

                // Hashtags are saved as a single string
                // each hashtag is seperated by a "/"
                // Split single hashtag string into substrings

                String value = tweet.Hashtags;
                Char delimiter = '/';
                String[] substrings = value.Split(delimiter);

                // Look at each hashtag in the tweet
                // if the hashtag is new create a hashtagStringCount object for it
                // keep track of the number of times each hashtag has been used
                // Using the HashtagString here to limit having to go back to the database to retrieve each Hashtag object
                // Will retrieve hashtag object once the top ten have been found

                foreach (String hashtagString in substrings)
                {
                    if (hashtagStrings != null && hashtagStrings.Contains(hashtagString))
                    {
                        HashtagStringCount hashtagStringCount = hashtagStringCounts.Single(hsc => hsc.HashtagString == hashtagString);
                        hashtagStringCount.TimesUsed += 100;
                    }
                    else
                    {
                        HashtagStringCount newHashtagStringCount = new HashtagStringCount
                        {
                            HashtagString = hashtagString,
                            TimesUsed = 100
                        };
                        hashtagStrings.Add(hashtagString);
                        hashtagStringCounts.Add(newHashtagStringCount);
                    }
                }
            }

            // Sort HashtagStringCounts by TimesUsed
            hashtagStringCounts.Sort(new HashtagStringCountComparer());

            if (hashtagStringCounts.Count > 11)
            {
                // Drop all but Top 10 Hashtags
                hashtagStringCounts.RemoveRange(11, hashtagStringCounts.Count - 11);
            }

            // Create a HashtagCount object for each  of the Top Ten HashtagStringCount objects
            // HashtagCount contains instance of hashtag object rather than just the string

            List<HashtagCount> hashtagCounts = new List<HashtagCount>();
            foreach (var hashtagStringCount in hashtagStringCounts)
            {
                Hashtag newHashtag = context.Hashtags.Single(ht => ht.Name == hashtagStringCount.HashtagString);
                HashtagCount newHashtagCount = new HashtagCount
                {
                    Hashtag = newHashtag,
                    TimesUsed = hashtagStringCount.TimesUsed
                };
                hashtagCounts.Add(newHashtagCount);
            }
            // Sort HashtagCounts by TimesUsed
            hashtagCounts.Sort(new HashtagCountComparer());
            languageCounts.Sort(new LanguageCountComparer());

            int totalLanguages = languagesIdsUsed.Count;
            int totalHashtags = hashtagStrings.Count -1;
            string hashtagName = hashtag.Name;


            HashtagViewModel hashtagViewModel = new HashtagViewModel(languageCounts, hashtagCounts, tweetsPerHour,
                totalTweets, totalLanguages, totalHashtags, hashtagName);

            return View(hashtagViewModel);
        }

        // Take in unix time and return DateTime object
        public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}