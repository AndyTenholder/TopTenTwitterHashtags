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
                // TODO: Null object reference exception on tweetHashtag.Hashtag.Name
                // tweetHashtag.Hashtag.Get returning null
                if (hourHashtags != null && hourHashtags.Contains(tweetHashtag.Hashtag.Name))
                {
                    HashtagCount hashtagCount = hourHashtagCounts.First(hc => hc.Hashtag == tweetHashtag.Hashtag);
                    // Twitter API stream only gives 1% of all tweets
                    // So each hashtag collected will be counted 100 times
                    hashtagCount.TimesUsed += 100;
                }
                else
                {
                    HashtagCount newHashtagCount = new HashtagCount
                    {
                        Hashtag = tweetHashtag.Hashtag,
                        // Twitter API stream only gives 1% of all tweets
                        // So each hashtag collected will be counted 100 times
                        TimesUsed = 100
                    };
                    hourHashtagCounts.Add(newHashtagCount);
                    hourHashtags.Add(tweetHashtag.Hashtag.Name);
                }
            }
            
            // Sort HashtagCounts by TimesUsed
            hourHashtagCounts.Sort(new HashtagCountComparer());
            hourHashtagCounts.RemoveRange(10, hourHashtagCounts.Count - 10);

            // TODO Create an list of arrays for each HourHashtagCount broken down into 5 minute intrevals

            HomeViewModel homeViewModel = new HomeViewModel(hourHashtagCounts);

            return View(homeViewModel);
        }
    }
}
