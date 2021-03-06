﻿using Microsoft.EntityFrameworkCore;
using TwitterWebMVCv2.Models;

namespace TwitterWebMVCv2.Data
{
    public class TweetDbContext : DbContext
    {
        public DbSet<Language> Languages { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<TweetHashtag> TweetHashtags { get; set; }

        public TweetDbContext(DbContextOptions<TweetDbContext> options)
            : base(options)
        { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"***********");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TweetHashtag>()
                .HasIndex(tht => tht.HashtagID);
            modelBuilder.Entity<Tweet>()
                .HasIndex(t => t.UnixTimeStamp);
            modelBuilder.Entity<Hashtag>()
                .HasIndex(h => h.ID);
        }
    }
}
