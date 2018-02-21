using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TwitterWebMVCv2.Models;
using TwitterWebMVCv2.Data;
using TwitterWebMVCv2.CountObjects;
using TwitterWebMVCv2.Comparers;
using TwitterWebMVCv2.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TwitterWebMVCv2.Controllers
{
    public class HomeController : Controller
    {

        private TweetDbContext context;

        DateTime dateTimeNow = DateTime.UtcNow;

        public HomeController(TweetDbContext dbContext)
        {
            context = dbContext;
        }

        public IActionResult Index()
        {

            List<HashtagCount> hourTopTen = GetTopTenHour();
            //List<HashtagCount> dayTopTen = GetTopTenDay();
            //List<HashtagCount> weekTopTen = GetTopTenWeek();

            TopTenViewModel topTenViewModel = new TopTenViewModel(hourTopTen);

            return View(topTenViewModel);
        }

        public IActionResult Hashtag(Hashtag hashtag)
        {
            IEnumerable<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.HashtagID == hashtag.ID).ToList();
            IEnumerable<TweetHashtag> tweethHashtagsPast24Hours = tweetHashtags.Where(th => UnixTimeStampToDateTime(th.UnixTimeStamp) > dateTimeNow.AddDays(-1));
            List<Tweet> tweets = new List<Tweet>();

            foreach (TweetHashtag th in tweethHashtagsPast24Hours)
            {
                Tweet tweet = context.Tweets.Single(t => t.ID == th.TweetID);
                tweets.Add(tweet);
            }

            int totalTweets = tweets.Count * 100;

            // Get total number of languages
            List<Language> languages = context.Languages.ToList();
            int totalLanguages = languages.Count;

            // Sort languages by TimesUsed
            languages.Sort(new LanguageComparer());
            foreach (var language in languages)
            {
                language.TimesUsed = language.TimesUsed * 100;
            }

            // Get total number of Hashtags
            List<Hashtag> hashtags = context.Hashtags.ToList();
            int totalHashtags = hashtags.Count - 1;

            // Sort hashtags by TimesUsed
            hashtags.Sort(new HashtagComparer());
            foreach (var hashtag in hashtags)
            {
                hashtag.TimesUsed = hashtag.TimesUsed * 100;
            }

            // Create array to hold number of tweets in each hour
            int[] tweetsPerHour = new int[24];

            foreach (var tweet in tweets)
            {
                // Get number of Tweets for each hour
                int hour = UnixTimeStampToDateTime(tweet.UnixTimeStamp).Hour;
                tweetsPerHour[hour] += 100;
            }

            HashtagViewModel hashtagViewModel = new HashtagViewModel(languages, hashtags, tweetsPerHour,
                totalTweets, totalLanguages, totalHashtags);

            return View(hashtagViewModel);
        }

            private List<HashtagCount> GetTopTenHour()
        {
            DateTime dateTimeMinusHour = dateTimeNow.AddHours(-1);
            Int32 unixTimestampMinusHour = (Int32)(dateTimeMinusHour.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var tweetHashtags = context.TweetHashtags.FromSql("SELECT * FROM TweetHashtags WHERE UnixTimeStamp>{0}", unixTimestampMinusHour).ToList();
            IEnumerable<Tweet> tweets = context.Tweets.Where(t => t.UnixTimeStamp > unixTimestampMinusHour).ToList();
            IEnumerable<int> tweetHashtagIds = (tweetHashtags.Select(tht => tht.HashtagID)).ToList();
            IEnumerable<Hashtag> hashTags = context.Hashtags.Where(ht => tweetHashtagIds.Contains(ht.ID)).ToList(); ;
                      
            // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
            // HashtagCounts keep track of the number of times a hashtag was used
            List<HashtagCount> hashtagCounts = new List<HashtagCount>();

            // List to keep track of which hashtags already have a HashtagCount object
            // Will not be passed to ViewModel
            List<string> hashtagStringList = new List<string>();
            List<int> hashtagIdList = new List<int>();
            List<HashtagIdCount> hashtagIdCounts = new List<HashtagIdCount>();

            foreach(TweetHashtag tweetHashtag in tweetHashtags)
            {
                if (hashtagIdList != null && hashtagIdList.Contains(tweetHashtag.HashtagID))
                {
                    HashtagIdCount hashtagIdCount = hashtagIdCounts.Single(hid => hid.HashtagId == tweetHashtag.HashtagID);
                    hashtagIdCount.TimesUsed += 1;
                    hashtagIdCount.TweetHashtags.Add(tweetHashtag);
                }
                else
                {
                    HashtagIdCount newHashtagIdCount = new HashtagIdCount
                    {
                        HashtagId = tweetHashtag.HashtagID,
                        TimesUsed = 1,
                        TweetHashtags = new List<TweetHashtag>()
                    };
                    newHashtagIdCount.TweetHashtags.Add(tweetHashtag);
                    hashtagIdCounts.Add(newHashtagIdCount);
                    hashtagIdList.Add(newHashtagIdCount.HashtagId);
                }
            }

            // Sort HashtagCounts by TimesUsed
            hashtagIdCounts.Sort(new HashtagIdCountComparer());

            if (hashtagIdCounts.Count > 10)
            {
                // Drop all but Top 10 Hashtags
                hashtagIdCounts.RemoveRange(10, hashtagIdCounts.Count - 10);
            }

            foreach (HashtagIdCount hashtagIdCount in hashtagIdCounts)
            {
                foreach (TweetHashtag tweetHashtag in hashtagIdCount.TweetHashtags)
                {
                    Hashtag hashTag = hashTags.Single(h => h.ID == tweetHashtag.HashtagID);

                    if (hashtagStringList != null && hashtagStringList.Contains(hashTag.Name.ToString()))
                    {
                        HashtagCount hashtagCount = hashtagCounts.Single(hc => hc.Hashtag == hashTag);
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        hashtagCount.TimesUsed += 100;
                        AddTweetPerMinute(tweetHashtag, hashtagCount, tweets);
                    }
                    else
                    {
                        HashtagCount newHashtagCount = new HashtagCount
                        {
                            Hashtag = hashTag,
                            // Twitter API stream only gives 1% of all tweets
                            // So each hashtag collected will be counted 100 times
                            TimesUsed = 100,

                            // int[] that stores data to be used in line graph
                            // period is intervals for the X axis
                            TweetsPer = new int[12]
                        };
                        // adds 100 to the correct 5 minute interval 0-11
                        Tweet tweet = tweets.SingleOrDefault(t => t.ID == tweetHashtag.TweetID);
                        if (tweet != null)
                        {
                            newHashtagCount.TweetsPer[Convert.ToInt32(Math.Floor((dateTimeNow - UnixTimeStampToDateTime(tweet.UnixTimeStamp)).Minutes / 5.0))] += 100;
                        }

                        hashtagCounts.Add(newHashtagCount);
                        hashtagStringList.Add(hashTag.Name);
                    }
                }
            }
            

            // Sort HashtagCounts by TimesUsed
            hashtagCounts.Sort(new HashtagCountComparer());

            if (hashtagCounts.Count > 10)
            {
                // Drop all but Top 10 Hashtags
                hashtagCounts.RemoveRange(10, hashtagCounts.Count - 10);
            }
            
            return hashtagCounts;
        }

        public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount, IEnumerable<Tweet> tweets)
        {

            Tweet tweet = tweets.SingleOrDefault(t => t.ID == tweetHashtag.TweetID);
            if (tweet != null)
            {
                hashtagCount.TweetsPer[Convert.ToInt32(Math.Floor((dateTimeNow - UnixTimeStampToDateTime(tweet.UnixTimeStamp)).Minutes / 5.0))] += 100;
            }
        }

        //    private List<HashtagCount> GetTopTenDay()
        //    {
        //        // Find TweetHashtags created in the past day
        //        List<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > dateTimeNow.AddDays(-1)).ToList(); ;

        //        // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
        //        // HashtagCounts keep track of the number of times a hashtag was used
        //        List<HashtagCount> hashtagCounts = new List<HashtagCount>();

        //        // List to keep track of which hashtags already have a HashtagCount object
        //        // Will not be passed to ViewModel
        //        List<string> hashtagStringList = new List<string>();

        //        foreach (TweetHashtag tweetHashtag in tweetHashtags)
        //        {
        //            Hashtag hashTag = context.Hashtags.First(h => h.ID == tweetHashtag.HashtagID);

        //            if (hashtagStringList != null && hashtagStringList.Contains(hashTag.Name))
        //            {
        //                HashtagCount hashtagCount = hashtagCounts.First(hc => hc.Hashtag == hashTag);

        //                // Twitter API stream only gives 1% of all tweets
        //                // So each hashtag collected will be counted 100 times
        //                hashtagCount.TimesUsed += 100;

        //                // adds 100 to the correct hour interval 0-23
        //                AddTweetPerHour(tweetHashtag, hashtagCount);

        //            }
        //            else
        //            {
        //                HashtagCount newHashtagCount = new HashtagCount
        //                {
        //                    Hashtag = tweetHashtag.Hashtag,
        //                    // Twitter API stream only gives 1% of all tweets
        //                    // So each hashtag collected will be counted 100 times
        //                    TimesUsed = 100,

        //                    // int[] that stores data to be used in line graph
        //                    // period is intervals for the X axis
        //                    TweetsPer = new int[24]
        //                };
        //                // adds 100 to the correct hour interval 0-23
        //                AddTweetPerHour(tweetHashtag, newHashtagCount);

        //                hashtagCounts.Add(newHashtagCount);
        //                hashtagStringList.Add(tweetHashtag.Hashtag.Name);
        //            }
        //        }

        //        // Sort HashtagCounts by TimesUsed
        //        hashtagCounts.Sort(new HashtagCountComparer());
        //        // Drop all but Top 10 Hashtags
        //        hashtagCounts.RemoveRange(10, hashtagCounts.Count - 10);

        //        return hashtagCounts;
        //    }

        //    private List<HashtagCount> GetTopTenWeek()
        //    {
        //        // Find TweetHashtags created in the past day
        //        List<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > dateTimeNow.AddDays(-7)).ToList(); ;

        //        // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
        //        // HashtagCounts keep track of the number of times a hashtag was used
        //        List<HashtagCount> hashtagCounts = new List<HashtagCount>();

        //        // List to keep track of which hashtags already have a HashtagCount object
        //        // Will not be passed to ViewModel
        //        List<string> hashtagStringList = new List<string>();

        //        foreach (TweetHashtag tweetHashtag in tweetHashtags)
        //        {
        //            Hashtag hashTag = context.Hashtags.First(h => h.ID == tweetHashtag.HashtagID);

        //            if (hashtagStringList != null && hashtagStringList.Contains(hashTag.Name))
        //            {
        //                HashtagCount hashtagCount = hashtagCounts.First(hc => hc.Hashtag == hashTag);

        //                // Twitter API stream only gives 1% of all tweets
        //                // So each hashtag collected will be counted 100 times
        //                hashtagCount.TimesUsed += 100;

        //                // adds 100 to the correct hour interval 0-23
        //                AddTweetPerDay(tweetHashtag, hashtagCount);

        //            }
        //            else
        //            {
        //                HashtagCount newHashtagCount = new HashtagCount
        //                {
        //                    Hashtag = tweetHashtag.Hashtag,
        //                    // Twitter API stream only gives 1% of all tweets
        //                    // So each hashtag collected will be counted 100 times
        //                    TimesUsed = 100,

        //                    // int[] that stores data to be used in line graph
        //                    // period is intervals for the X axis
        //                    TweetsPer = new int[7]
        //                };
        //                // adds 100 to the correct hour interval 0-23
        //                AddTweetPerDay(tweetHashtag, newHashtagCount);

        //                hashtagCounts.Add(newHashtagCount);
        //                hashtagStringList.Add(tweetHashtag.Hashtag.Name);
        //            }
        //        }

        //        // Sort HashtagCounts by TimesUsed
        //        hashtagCounts.Sort(new HashtagCountComparer());
        //        // Drop all but Top 10 Hashtags
        //        hashtagCounts.RemoveRange(10, hashtagCounts.Count - 10);

        //        return hashtagCounts;
        //    }

        //    // Determine how many days ago the tweet occured and add 100 to the index of the day
        //    private void AddTweetPerDay(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        //    {
        //        Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
        //        hashtagCount.TweetsPer[(dateTimeNow - tweet.DateTime).Days] += 100;
        //    }

        //    // Determine how many hours ago the tweet occured and add 100 to the index of the hour
        //    private void AddTweetPerHour(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        //    {
        //        Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
        //        hashtagCount.TweetsPer[(dateTimeNow - tweet.DateTime).Hours] += 100;
        //    }

        // Determine how many minutes ago the tweet occured and add 100 to the index of the minute range


    }
}