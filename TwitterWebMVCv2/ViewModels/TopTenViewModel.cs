using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterWebMVCv2.CountObjects;
using TwitterWebMVCv2.Models;

namespace TwitterWebMVCv2.ViewModels
{
    public class TopTenViewModel
    {
        public List<HashtagCount> HourHashtagCounts { get; set; }
        public List<Hashtag> HashtagsAll { get; set; }
        public DateTime DateTimeNow { get; set; }
        public DateTime MyProperty { get; set; }

        public TopTenViewModel(List<HashtagCount> hourHashtagCounts, List<Hashtag> hashtagsAll, DateTime dateTimeNow)
        {
            HourHashtagCounts = hourHashtagCounts;
            HashtagsAll = hashtagsAll;
            DateTimeNow = dateTimeNow;
        }
    }
}
