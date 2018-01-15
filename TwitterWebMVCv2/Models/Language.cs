using System.Collections.Generic;

namespace TwitterWebMVCv2.Models
{
    public class Language
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public IList<Tweet> Tweets { get; set; }

    }
}
