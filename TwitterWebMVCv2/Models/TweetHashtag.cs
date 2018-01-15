
namespace TwitterWebMVCv2.Models
{
    public class TweetHashtag
    {
        public int TweetID { get; set; }
        public Tweet Tweet { get; set; }

        public int HashtagID { get; set; }
        public Hashtag Hashtag { get; set; }

        public TweetHashtag() { }
    }
}
