using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterWebMVCv2.CountObjects;

namespace TwitterWebMVCv2.ViewModels
{
    public class HomeViewModel
    {
        public List<HashtagCount> HourHashtagCounts { get; set; }
        public List<HashtagCount> DayHashtagCounts { get; set; }
        public List<HashtagCount> WeekHashtagCounts { get; set; }

        public HomeViewModel(List<HashtagCount> hourHashtagCounts, List<HashtagCount> dayHashtagCounts, List<HashtagCount> weekHashtagCounts)
        {
            HourHashtagCounts = hourHashtagCounts;
            DayHashtagCounts = dayHashtagCounts;
            WeekHashtagCounts = weekHashtagCounts;
        }

        public HomeViewModel(List<HashtagCount> hourHashtagCounts)
        {
            HourHashtagCounts = hourHashtagCounts;
        }
    }
}
