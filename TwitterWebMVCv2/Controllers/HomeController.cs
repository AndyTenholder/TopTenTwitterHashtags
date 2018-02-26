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
using System.Text;

namespace TwitterWebMVCv2.Controllers
{
    public class HomeController : Controller
    {

        private TweetDbContext context;

        // All timestamps are in UTC time.  So UTC is used throughout the app

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

        // Method to retrun Top Ten hashtags of the past hour
            private List<HashtagCount> GetTopTenHour()
        {
            // Create variable to hold unix time minus one hour
            // Time stamps in DB are in unix time
            DateTime dateTimeMinusHour = dateTimeNow.AddHours(-1);
            Int32 unixTimestampMinusHour = (Int32)(dateTimeMinusHour.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            // Contact DB and retrieve data needed
            var tweetHashtags = context.TweetHashtags.FromSql("SELECT * FROM TweetHashtags WHERE UnixTimeStamp>{0}", unixTimestampMinusHour).ToList();
            IEnumerable<Tweet> tweets = context.Tweets.Where(t => t.UnixTimeStamp > unixTimestampMinusHour).ToList();
            IEnumerable<int> tweetHashtagIds = (tweetHashtags.Select(tht => tht.HashtagID)).ToList();
            IEnumerable<Hashtag> hashTags = context.Hashtags.Where(ht => tweetHashtagIds.Contains(ht.ID)).ToList(); ;

            // HashtagIdCount save the ID of the hashtag, keep track of the number of times it was used, and save a list of the TweetHashtags
            List<HashtagIdCount> hashtagIdCounts = new List<HashtagIdCount>();

            // listof hashtagIds will be used to check if a hashtagIdCount has already been made for that hashtag
            List<int> hashtagIdList = new List<int>();

            // For each unique hashtag create HashtagIdCount object and keep track of how many times it was used 
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

            // Sort HashtagIdCounts by TimesUsed
            hashtagIdCounts.Sort(new HashtagIdCountComparer());


            if (hashtagIdCounts.Count > 10)
            {
                // Drop all but Top 10 Hashtags
                hashtagIdCounts.RemoveRange(10, hashtagIdCounts.Count - 10);
            }

            // HastagCount objects contain a Hashtag object, TimesUsed int, and an int[] TweetsPer
            // HashtagCounts will be used to hold information about how many times the hashtag was used in 5 minute intervals
            List<HashtagCount> hashtagCounts = new List<HashtagCount>();

            // List to keep track of which hashtags already have a HashtagCount object
            // Will not be passed to ViewModel
            List<string> hashtagStringList = new List<string>();

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
                        byte[] bytes = Encoding.Default.GetBytes(hashTag.Name);
                        string hashtagName = (Encoding.UTF8.GetString(bytes));

                        HashtagCount newHashtagCount = new HashtagCount
                        {
                            Hashtag = hashTag,
                            // Twitter API stream only gives 1% of all tweets
                            // So each hashtag collected will be counted 100 times
                            TimesUsed = 100,

                            HashtagName = hashtagName,

                            // int[] that stores data to be used in line graph
                            // period is intervals for the X axis
                            TweetsPer = new int[12]
                        };

                        // TODO- tweet is sometimes returning as null, not sure how this is happening
                        // using if to catch null instances
                        Tweet tweet = tweets.SingleOrDefault(t => t.ID == tweetHashtag.TweetID);
                        if (tweet != null)
                        {
                            // adds 100 to the correct 5 minute interval 0-11
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

        // Take in unix time and return DateTime object
        public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void AddTweetPerMinute(TweetHashtag tweetHashtag, HashtagCount hashtagCount, IEnumerable<Tweet> tweets)
        {
            // TODO- tweet is sometimes returning as null, not sure how this is happening
            // using if to catch null instances
            Tweet tweet = tweets.SingleOrDefault(t => t.ID == tweetHashtag.TweetID);
            if (tweet != null)
            {
                // adds 100 to the correct 5 minute interval 0-11
                hashtagCount.TweetsPer[Convert.ToInt32(Math.Floor((dateTimeNow - UnixTimeStampToDateTime(tweet.UnixTimeStamp)).Minutes / 5.0))] += 100;
            }
        }
    }
}