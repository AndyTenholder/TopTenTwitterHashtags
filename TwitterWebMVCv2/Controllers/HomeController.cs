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
            List<HashtagCount> dayTopTen = GetTopTen("day");
            List<HashtagCount> weekTopTen = GetTopTen("week");

            HomeViewModel homeViewModel = new HomeViewModel(hourTopTen, dayTopTen, weekTopTen);

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
            if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-1))
            {
                hashtagCount.TweetsPer[6] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-2))
            {
                hashtagCount.TweetsPer[5] += 100;
            }
            else if(DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-3))
            {
                hashtagCount.TweetsPer[4] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-4))
            {
                hashtagCount.TweetsPer[3] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-5))
            {
                hashtagCount.TweetsPer[2] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-6))
            {
                hashtagCount.TweetsPer[1] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-7))
            {
                hashtagCount.TweetsPer[0] += 100;
            }
        }

        // Determine which hour the tweet occured on and add 100 to the index of the hour
        private void AddTweetPerHour(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            if (tweetHashtag.Tweet.DateTime.Hour == 0)
            {
                hashtagCount.TweetsPer[0] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 1)
            {
                hashtagCount.TweetsPer[1] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 2)
            {
                hashtagCount.TweetsPer[2] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 3)
            {
                hashtagCount.TweetsPer[3] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 4)
            {
                hashtagCount.TweetsPer[4] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 5)
            {
                hashtagCount.TweetsPer[5] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 6)
            {
                hashtagCount.TweetsPer[6] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 7)
            {
                hashtagCount.TweetsPer[7] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 8)
            {
                hashtagCount.TweetsPer[8] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 9)
            {
                hashtagCount.TweetsPer[9] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 10)
            {
                hashtagCount.TweetsPer[10] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 11)
            {
                hashtagCount.TweetsPer[11] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 12)
            {
                hashtagCount.TweetsPer[12] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 13)
            {
                hashtagCount.TweetsPer[13] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 14)
            {
                hashtagCount.TweetsPer[14] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 15)
            {
                hashtagCount.TweetsPer[15] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 16)
            {
                hashtagCount.TweetsPer[16] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 17)
            {
                hashtagCount.TweetsPer[17] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 18)
            {
                hashtagCount.TweetsPer[18] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 19)
            {
                hashtagCount.TweetsPer[19] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 20)
            {
                hashtagCount.TweetsPer[20] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 21)
            {
                hashtagCount.TweetsPer[21] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 22)
            {
                hashtagCount.TweetsPer[22] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 23)
            {
                hashtagCount.TweetsPer[23] += 100;
            }
        }

        // Determine which minute range the tweet occured on and add 100 to the index of the minute range
        private void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            if (tweetHashtag.Tweet.DateTime.Minute < 6)
            {
                hashtagCount.TweetsPer[0] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 11)
            {
                hashtagCount.TweetsPer[1] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 16)
            {
                hashtagCount.TweetsPer[2] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 21)
            {
                hashtagCount.TweetsPer[3] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 26)
            {
                hashtagCount.TweetsPer[4] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 31)
            {
                hashtagCount.TweetsPer[5] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 36)
            {
                hashtagCount.TweetsPer[6] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 41)
            {
                hashtagCount.TweetsPer[7] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 46)
            {
                hashtagCount.TweetsPer[8] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 51)
            {
                hashtagCount.TweetsPer[9] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 55)
            {
                hashtagCount.TweetsPer[10] += 100;
            }
            else
            {
                hashtagCount.TweetsPer[11] += 100;
            }
        }
    }
}
