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

        public HomeController(TweetDbContext dbContext)
        {
            context = dbContext;
        }
        public IActionResult Index()
        {

            List<HashtagCount> hourTopTen = GetTopTen("hour");
            //List<HashtagCount> dayTopTen = GetTopTen("day");
            //List<HashtagCount> weekTopTen = GetTopTen("week");

            HomeViewModel homeViewModel = new HomeViewModel(hourTopTen);

            return View(homeViewModel);
        }

        private List<HashtagCount> GetTopTen(string interval)
        {
            List<TweetHashtag> tweetHashtags = new List<TweetHashtag>();

            if (interval == "hour")
            {
                // Find TweetHashtags created in the past hour
                tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > DateTime.Now.AddHours(-1)).ToList();
            }
            if (interval == "day")
            {
                // Find TweetHashtags created in the past day
                tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > DateTime.Now.AddDays(-1)).ToList();
            }
            if (interval == "week")
            {
                // Find TweetHashtags created in the past day
                tweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > DateTime.Now.AddDays(-7)).ToList();
            }


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
                    if (interval == "hour")
                    {
                        // adds 100 to the correct 5 minute interval 0-11
                        AddTweetPerMinute(tweetHashtag, hashtagCount);
                    }
                    if (interval == "day")
                    {
                        // adds 100 to the correct hour interval 0-23
                        AddTweetPerHour(tweetHashtag, hashtagCount);
                    }
                    if (interval == "week")
                    {
                        // adds 100 to the correct day interval 0-6
                        AddTweetPerDay(tweetHashtag, hashtagCount);
                    }
                }
                else
                {
                    // period be the interval on the x-axis of the line graph
                    int period = 0;
                    if (interval == "hour")
                    {
                        // Each hour will be divided into 5 minute intervals
                        period = 12;
                    }
                    if (interval == "day")
                    {
                        // Each day will be divided into 1 hour intervals
                        period = 24;
                    }
                    if (interval == "week")
                    {
                        // Each week will be divided into 1 day intervals
                        period = 7;
                    }
                    HashtagCount newHashtagCount = new HashtagCount
                    {
                        Hashtag = tweetHashtag.Hashtag,
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        TimesUsed = 100,

                        // int[] that stores data to be used in line graph
                        // period is intervals for the X axis
                        TweetsPer = new int[period]
                    };
                    if (interval == "hour")
                    {
                        // adds 100 to the correct 5 minute interval 0-11
                        AddTweetPerMinute(tweetHashtag, newHashtagCount);
                    }
                    if (interval == "day")
                    {
                        // adds 100 to the correct hour interval 0-23
                        AddTweetPerHour(tweetHashtag, newHashtagCount);
                    }
                    if (interval == "week")
                    {
                        // adds 100 to the correct day interval 0-6
                        AddTweetPerDay(tweetHashtag, newHashtagCount);
                    }

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

        // Determine which day the tweet occured on and add 100 to the index of the day
        private void AddTweetPerDay(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
            var tweetDateTime = tweet.DateTime;
            var nowDateTime = DateTime.Now;
            var tweetsPer = hashtagCount.TweetsPer;

            if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-1))
            {
                tweetsPer[6] += 100;
            }
            else if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-2))
            {
                tweetsPer[5] += 100;
            }
            else if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-3))
            {
                tweetsPer[4] += 100;
            }
            else if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-4))
            {
                tweetsPer[3] += 100;
            }
            else if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-5))
            {
                tweetsPer[2] += 100;
            }
            else if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-6))
            {
                tweetsPer[1] += 100;
            }
            else if (nowDateTime - tweetDateTime < nowDateTime - nowDateTime.AddDays(-7))
            {
                tweetsPer[0] += 100;
            }
        }

        // Determine which hour the tweet occured on and add 100 to the index of the hour
        private void AddTweetPerHour(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
            int tweetHour = tweet.DateTime.Hour;
            var tweetsPer = hashtagCount.TweetsPer;

            switch (tweetHour)
            {
                case 0:
                    tweetsPer[0] += 100;
                    break;
                case 1:
                    tweetsPer[1] += 100;
                    break;
                case 2:
                    tweetsPer[2] += 100;
                    break;
                case 3:
                    tweetsPer[3] += 100;
                    break;
                case 4:
                    tweetsPer[4] += 100;
                    break;
                case 5:
                    tweetsPer[5] += 100;
                    break;
                case 6:
                    tweetsPer[6] += 100;
                    break;
                case 7:
                    tweetsPer[7] += 100;
                    break;
                case 8:
                    tweetsPer[8] += 100;
                    break;
                case 9:
                    tweetsPer[9] += 100;
                    break;
                case 10:
                    tweetsPer[10] += 100;
                    break;
                case 11:
                    tweetsPer[11] += 100;
                    break;
                case 12:
                    tweetsPer[12] += 100;
                    break;
                case 13:
                    tweetsPer[13] += 100;
                    break;
                case 14:
                    tweetsPer[14] += 100;
                    break;
                case 15:
                    tweetsPer[15] += 100;
                    break;
                case 16:
                    tweetsPer[16] += 100;
                    break;
                case 17:
                    tweetsPer[17] += 100;
                    break;
                case 18:
                    tweetsPer[18] += 100;
                    break;
                case 19:
                    tweetsPer[19] += 100;
                    break;
                case 20:
                    tweetsPer[20] += 100;
                    break;
                case 21:
                    tweetsPer[21] += 100;
                    break;
                case 22:
                    tweetsPer[22] += 100;
                    break;
                case 23:
                    tweetsPer[23] += 100;
                    break;
            }
        }

        // Determine which minute range the tweet occured on and add 100 to the index of the minute range
        private void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            Tweet tweet = context.Tweets.First(t => t.ID == tweetHashtag.TweetID);
            var tweetMinute = tweet.DateTime.Minute;
            var tweetsPer = hashtagCount.TweetsPer;

            if (tweetMinute < 6)
            {
                tweetsPer[0] += 100;
            }
            else if (tweetMinute < 11)
            {
                tweetsPer[1] += 100;
            }
            else if (tweetMinute < 16)
            {
                tweetsPer[2] += 100;
            }
            else if (tweetMinute < 21)
            {
                tweetsPer[3] += 100;
            }
            else if (tweetMinute < 26)
            {
                tweetsPer[4] += 100;
            }
            else if (tweetMinute < 31)
            {
                tweetsPer[5] += 100;
            }
            else if (tweetMinute < 36)
            {
                tweetsPer[6] += 100;
            }
            else if (tweetMinute < 41)
            {
                tweetsPer[7] += 100;
            }
            else if (tweetMinute < 46)
            {
                tweetsPer[8] += 100;
            }
            else if (tweetMinute < 51)
            {
                tweetsPer[9] += 100;
            }
            else if (tweetMinute < 55)
            {
                tweetsPer[10] += 100;
            }
            else
            {
                tweetsPer[11] += 100;
            }
        }
    }
}