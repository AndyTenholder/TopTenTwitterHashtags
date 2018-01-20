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

            List<HashtagCount> hourTopTen = GetHourTopTen();
           

            // TODO Create an list of arrays for each HourHashtagCount broken down into 5 minute intrevals

            HomeViewModel homeViewModel = new HomeViewModel(hourTopTen);

            return View(homeViewModel);
        }

        public List<HashtagCount> GetHourTopTen()
        {
            // Create list of TweetHashtags created in the past hour
            List<TweetHashtag> hourTweetHashtags = context.TweetHashtags.Where(th => th.Tweet.DateTime > DateTime.Now.AddHours(-1)).ToList();

            // HastagCount objects contain a Hashtag object and TimesUsed int
            // HashtagCounts keep track of the number of times a hashtag was used
            List<HashtagCount> hourHashtagCounts = new List<HashtagCount>();

            // List to keep track of which hashtags already have a HashtagCount object
            // Will not be passed to ViewModel
            List<string> hourHashtags = new List<string>();

            foreach (TweetHashtag tweetHashtag in hourTweetHashtags)
            {
                Hashtag hashTag = context.Hashtags.First(h => h.ID == tweetHashtag.HashtagID);

                if (hourHashtags != null && hourHashtags.Contains(hashTag.Name))
                {
                    HashtagCount hashtagCount = hourHashtagCounts.First(hc => hc.Hashtag == hashTag);
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
                        TweetsPerMinute = new int[12]
                    };
                    AddTweetPerMinute(tweetHashtag, newHashtagCount);

                    hourHashtagCounts.Add(newHashtagCount);
                    hourHashtags.Add(tweetHashtag.Hashtag.Name);
                }
            }

            // Sort HashtagCounts by TimesUsed
            hourHashtagCounts.Sort(new HashtagCountComparer());
            hourHashtagCounts.RemoveRange(10, hourHashtagCounts.Count - 10);

            return hourHashtagCounts;
        }

        public void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount)
        {
            if (tweetHashtag.Tweet.DateTime.Minute < 6)
            {
                hashtagCount.TweetsPerMinute[0] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 11)
            {
                hashtagCount.TweetsPerMinute[1] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 16)
            {
                hashtagCount.TweetsPerMinute[2] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 21)
            {
                hashtagCount.TweetsPerMinute[3] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 26)
            {
                hashtagCount.TweetsPerMinute[4] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 31)
            {
                hashtagCount.TweetsPerMinute[5] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 36)
            {
                hashtagCount.TweetsPerMinute[6] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 41)
            {
                hashtagCount.TweetsPerMinute[7] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 46)
            {
                hashtagCount.TweetsPerMinute[8] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 51)
            {
                hashtagCount.TweetsPerMinute[9] += 100;
            }
            else if (tweetHashtag.Tweet.DateTime.Minute < 55)
            {
                hashtagCount.TweetsPerMinute[10] += 100;
            }
            else
            {
                hashtagCount.TweetsPerMinute[11] += 100;
            }
        }
    }
}
