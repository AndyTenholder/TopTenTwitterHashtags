using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TwitterWebMVCv2.Models;
using TwitterWebMVCv2.Data;
using TwitterWebMVCv2.CountObjects;
using TwitterWebMVCv2.Comparers;
using TwitterWebMVCv2.ViewModels;


namespace TwitterWebMVCv2.Controllers
{
    public class HomeController : Controller
    {
        private TweetDbContext context;

        DateTime dateTimeNow = DateTime.Now;

        public HomeController(TweetDbContext dbContext)
        {
            context = dbContext;
        }
        public IActionResult Index()
        {

            List<HashtagCount> hourTopTen = GetTopTenHour();
            //List<HashtagCount> dayTopTen = GetTopTenDay();
            //List<HashtagCount> weekTopTen = GetTopTenWeek();

            HomeViewModel homeViewModel = new HomeViewModel(hourTopTen);

            return View(homeViewModel);
        }

        private List<HashtagCount> GetTopTenHour()
        {
            List<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > dateTimeNow.AddHours(-1)).ToList();
            List<Tweet> tweets = context.Tweets.Where(t => t.DateTime > dateTimeNow.AddHours(-1)).ToList();
            List<int> tweetHashtagIds = tweetHashtags.Select(tht => tht.HashtagID).ToList();
            List<Hashtag> hashTags = context.Hashtags.Where(ht => tweetHashtagIds.Contains(ht.ID)).ToList();

            // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
            // HashtagCounts keep track of the number of times a hashtag was used
            List<HashtagCount> hashtagCounts = new List<HashtagCount>();

            // List to keep track of which hashtags already have a HashtagCount object
            // Will not be passed to ViewModel
            List<string> hashtagStringList = new List<string>();

            foreach (TweetHashtag tweetHashtag in tweetHashtags)
            {
                Hashtag hashTag = hashTags.Single(h => h.ID == tweetHashtag.HashtagID);

                if (hashtagStringList != null && hashtagStringList.Contains(hashTag.Name.ToString()))
                {
                    HashtagCount hashtagCount = hashtagCounts.Single(hc => hc.Hashtag == hashTag);
                    // Twitter API stream only gives 1% of all tweets
                    // So each hashtag collected will be counted 100 times
                    hashtagCount.TimesUsed += 100;
                    AddTweetPerMinute(tweetHashtag, hashtagCount);
                }
                else
                {
                    HashtagCount newHashtagCount = new HashtagCount
                    {
                        Hashtag = tweetHashtag.Hashtag,
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        TimesUsed = 100,

                        // int[] that stores data to be used in line graph
                        // period is intervals for the X axis
                        TweetsPer = new int[12]
                    };
                    // adds 100 to the correct 5 minute interval 0-11
                    Tweet tweet = tweets.Single(t => t.ID == tweetHashtag.TweetID);
                    newHashtagCount.TweetsPer[Convert.ToInt32(Math.Floor((dateTimeNow - tweet.DateTime).Minutes / 5.0))] += 100;

                    hashtagCounts.Add(newHashtagCount);
                    hashtagStringList.Add(tweetHashtag.Hashtag.Name);
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

        private List<HashtagCount> GetTopTenDay()
        {
            // Find TweetHashtags created in the past day
            List<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > dateTimeNow.AddDays(-1)).ToList(); ;

            // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
            // HashtagCounts keep track of the number of times a hashtag was used
            List<HashtagCount> hashtagCounts = new List<HashtagCount>();

            // List to keep track of which hashtags already have a HashtagCount object
            // Will not be passed to ViewModel
            List<string> hashtagStringList = new List<string>();

            foreach (TweetHashtag tweetHashtag in tweetHashtags)
            {
                Hashtag hashTag = context.Hashtags.First(h => h.ID == tweetHashtag.HashtagID);

                if (hashtagStringList != null && hashtagStringList.Contains(hashTag.Name))
                {
                    HashtagCount hashtagCount = hashtagCounts.First(hc => hc.Hashtag == hashTag);
                    
                    // Twitter API stream only gives 1% of all tweets
                    // So each hashtag collected will be counted 100 times
                    hashtagCount.TimesUsed += 100;

                    // adds 100 to the correct hour interval 0-23
                    AddTweetPerHour(tweetHashtag, hashtagCount);

                }
                else
                {
                    HashtagCount newHashtagCount = new HashtagCount
                    {
                        Hashtag = tweetHashtag.Hashtag,
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        TimesUsed = 100,

                        // int[] that stores data to be used in line graph
                        // period is intervals for the X axis
                        TweetsPer = new int[24]
                    };
                    // adds 100 to the correct hour interval 0-23
                    AddTweetPerHour(tweetHashtag, newHashtagCount);

                    hashtagCounts.Add(newHashtagCount);
                    hashtagStringList.Add(tweetHashtag.Hashtag.Name);
                }
            }

            // Sort HashtagCounts by TimesUsed
            hashtagCounts.Sort(new HashtagCountComparer());
            // Drop all but Top 10 Hashtags
            hashtagCounts.RemoveRange(10, hashtagCounts.Count - 10);

            return hashtagCounts;
        }

        private List<HashtagCount> GetTopTenWeek()
        {
            // Find TweetHashtags created in the past day
            List<TweetHashtag> tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > dateTimeNow.AddDays(-7)).ToList(); ;

            // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
            // HashtagCounts keep track of the number of times a hashtag was used
            List<HashtagCount> hashtagCounts = new List<HashtagCount>();

            // List to keep track of which hashtags already have a HashtagCount object
            // Will not be passed to ViewModel
            List<string> hashtagStringList = new List<string>();

            foreach (TweetHashtag tweetHashtag in tweetHashtags)
            {
                Hashtag hashTag = context.Hashtags.First(h => h.ID == tweetHashtag.HashtagID);

                if (hashtagStringList != null && hashtagStringList.Contains(hashTag.Name))
                {
                    HashtagCount hashtagCount = hashtagCounts.First(hc => hc.Hashtag == hashTag);

                    // Twitter API stream only gives 1% of all tweets
                    // So each hashtag collected will be counted 100 times
                    hashtagCount.TimesUsed += 100;

                    // adds 100 to the correct hour interval 0-23
                    AddTweetPerDay(tweetHashtag, hashtagCount);

                }
                else
                {
                    HashtagCount newHashtagCount = new HashtagCount
                    {
                        Hashtag = tweetHashtag.Hashtag,
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        TimesUsed = 100,

                        // int[] that stores data to be used in line graph
                        // period is intervals for the X axis
                        TweetsPer = new int[7]
                    };
                    // adds 100 to the correct hour interval 0-23
                    AddTweetPerDay(tweetHashtag, newHashtagCount);

                    hashtagCounts.Add(newHashtagCount);
                    hashtagStringList.Add(tweetHashtag.Hashtag.Name);
                }
            }

            // Sort HashtagCounts by TimesUsed
            hashtagCounts.Sort(new HashtagCountComparer());
            // Drop all but Top 10 Hashtags
            hashtagCounts.RemoveRange(10, hashtagCounts.Count - 10);

            return hashtagCounts;
        }

        // Determine how many days ago the tweet occured and add 100 to the index of the day
        private void AddTweetPerDay(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
            hashtagCount.TweetsPer[(dateTimeNow - tweet.DateTime).Days] += 100;
        }

        // Determine how many hours ago the tweet occured and add 100 to the index of the hour
        private void AddTweetPerHour(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
            hashtagCount.TweetsPer[(dateTimeNow - tweet.DateTime).Hours] += 100;
        }

        // Determine how many minutes ago the tweet occured and add 100 to the index of the minute range
        private void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
            hashtagCount.TweetsPer[Convert.ToInt32(Math.Floor((dateTimeNow - tweet.DateTime).Minutes / 5.0))] += 100;
        }
    }
}