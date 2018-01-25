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
                    HashtagCount newHashtagCount = new HashtagCount
                    {
                        Hashtag = tweetHashtag.Hashtag,
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        TimesUsed = 100,

                        // int[] that stores data to be used in line graph
                        // period is intervals for the X axis
                        TweetsPer = new Dictionary<string, int>()
                    };
                    if (interval == "hour")
                    {
                        // Create dictionary elements for 5 minute intervals
                        newHashtagCount.TweetsPer.Add("5", 0);
                        newHashtagCount.TweetsPer.Add("10", 0);
                        newHashtagCount.TweetsPer.Add("15", 0);
                        newHashtagCount.TweetsPer.Add("20", 0);
                        newHashtagCount.TweetsPer.Add("25", 0);
                        newHashtagCount.TweetsPer.Add("30", 0);
                        newHashtagCount.TweetsPer.Add("35", 0);
                        newHashtagCount.TweetsPer.Add("40", 0);
                        newHashtagCount.TweetsPer.Add("45", 0);
                        newHashtagCount.TweetsPer.Add("50", 0);
                        newHashtagCount.TweetsPer.Add("55", 0);
                        newHashtagCount.TweetsPer.Add("60", 0);
                        // adds 100 to the correct minute key
                        AddTweetPerMinute(tweetHashtag, newHashtagCount);
                    }
                    if (interval == "day")
                    {
                        // Create dictionary elements for each hour in a day
                        newHashtagCount.TweetsPer.Add("12am", 0);
                        newHashtagCount.TweetsPer.Add("1am", 0);
                        newHashtagCount.TweetsPer.Add("2am", 0);
                        newHashtagCount.TweetsPer.Add("3am", 0);
                        newHashtagCount.TweetsPer.Add("4am", 0);
                        newHashtagCount.TweetsPer.Add("5am", 0);
                        newHashtagCount.TweetsPer.Add("6am", 0);
                        newHashtagCount.TweetsPer.Add("7am", 0);
                        newHashtagCount.TweetsPer.Add("9am", 0);
                        newHashtagCount.TweetsPer.Add("10am", 0);
                        newHashtagCount.TweetsPer.Add("11am", 0);
                        newHashtagCount.TweetsPer.Add("12pm", 0);
                        newHashtagCount.TweetsPer.Add("1pm", 0);
                        newHashtagCount.TweetsPer.Add("2pm", 0);
                        newHashtagCount.TweetsPer.Add("3pm", 0);
                        newHashtagCount.TweetsPer.Add("4pm", 0);
                        newHashtagCount.TweetsPer.Add("5pm", 0);
                        newHashtagCount.TweetsPer.Add("6pm", 0);
                        newHashtagCount.TweetsPer.Add("7pm", 0);
                        newHashtagCount.TweetsPer.Add("8pm", 0);
                        newHashtagCount.TweetsPer.Add("9pm", 0);
                        newHashtagCount.TweetsPer.Add("10pm", 0);
                        newHashtagCount.TweetsPer.Add("11pm", 0);

                        // adds 100 to the correct hour key
                        AddTweetPerHour(tweetHashtag, newHashtagCount);
                    }
                    if (interval == "week")
                    {
                        // Create dictionary elements for each days 1-7
                        newHashtagCount.TweetsPer.Add("1", 0);
                        newHashtagCount.TweetsPer.Add("2", 0);
                        newHashtagCount.TweetsPer.Add("3", 0);
                        newHashtagCount.TweetsPer.Add("4", 0);
                        newHashtagCount.TweetsPer.Add("5", 0);
                        newHashtagCount.TweetsPer.Add("6", 0);
                        newHashtagCount.TweetsPer.Add("7", 0);

                        // adds 100 to the correct day key
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
                hashtagCount.TweetsPer["7"] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-2))
            {
                hashtagCount.TweetsPer["6"] += 100;
            }
            else if(DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-3))
            {
                hashtagCount.TweetsPer["5"] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-4))
            {
                hashtagCount.TweetsPer["4"] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-5))
            {
                hashtagCount.TweetsPer["3"] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-6))
            {
                hashtagCount.TweetsPer["2"] += 100;
            }
            else if (DateTime.Now - tweetHashtag.Tweet.DateTime < DateTime.Now - DateTime.Now.AddDays(-7))
            {
                hashtagCount.TweetsPer["1"] += 100;
            }
        }

        // Determine which hour the tweet occured on and add 100 to the index of the hour
        private void AddTweetPerHour(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            if (tweetHashtag.Tweet.DateTime.Hour == 0)
            {
                hashtagCount.TweetsPer["12am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 1)
            {
                hashtagCount.TweetsPer["1am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 2)
            {
                hashtagCount.TweetsPer["2am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 3)
            {
                hashtagCount.TweetsPer["3am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 4)
            {
                hashtagCount.TweetsPer["4am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 5)
            {
                hashtagCount.TweetsPer["5am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 6)
            {
                hashtagCount.TweetsPer["6am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 7)
            {
                hashtagCount.TweetsPer["7am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 8)
            {
                hashtagCount.TweetsPer["8am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 9)
            {
                hashtagCount.TweetsPer["9am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 10)
            {
                hashtagCount.TweetsPer["10am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 11)
            {
                hashtagCount.TweetsPer["11am"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 12)
            {
                hashtagCount.TweetsPer["12pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 13)
            {
                hashtagCount.TweetsPer["1pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 14)
            {
                hashtagCount.TweetsPer["2pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 15)
            {
                hashtagCount.TweetsPer["3pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 16)
            {
                hashtagCount.TweetsPer["4pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 17)
            {
                hashtagCount.TweetsPer["5pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 18)
            {
                hashtagCount.TweetsPer["6pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 19)
            {
                hashtagCount.TweetsPer["7pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 20)
            {
                hashtagCount.TweetsPer["8pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 21)
            {
                hashtagCount.TweetsPer["9pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 22)
            {
                hashtagCount.TweetsPer["10pm"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Hour == 23)
            {
                hashtagCount.TweetsPer["11pm"] += 100;
            }
        }

        // Determine which minute range the tweet occured on and add 100 to the index of the minute range
        private void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            if (tweetHashtag.Tweet.DateTime.Minute < 6)
            {
                hashtagCount.TweetsPer["5"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 11)
            {
                hashtagCount.TweetsPer["10"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 16)
            {
                hashtagCount.TweetsPer["15"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 21)
            {
                hashtagCount.TweetsPer["20"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 26)
            {
                hashtagCount.TweetsPer["25"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 31)
            {
                hashtagCount.TweetsPer["30"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 36)
            {
                hashtagCount.TweetsPer["35"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 41)
            {
                hashtagCount.TweetsPer["40"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 46)
            {
                hashtagCount.TweetsPer["45"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 51)
            {
                hashtagCount.TweetsPer["50"] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 55)
            {
                hashtagCount.TweetsPer["55"] += 100;
            }
            else
            {
                hashtagCount.TweetsPer["60"] += 100;
            }
        }
    }
}
