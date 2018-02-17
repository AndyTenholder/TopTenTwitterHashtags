using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterWebMVCv2.CountObjects;

namespace TwitterWebMVCv2.Comparers
{
    public class HashtagIdCountComparer : IComparer<HashtagIdCount>
    {
        public int Compare(HashtagIdCount x, HashtagIdCount y)
        {
            if (x.TimesUsed < y.TimesUsed)
            {
                return 1;
            }
            if (x.TimesUsed > y.TimesUsed)
            {
                return -1;
            }
            return 0;
        }
    }
}
