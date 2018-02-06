using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Tweetinvi;
using Microsoft.Extensions.DependencyInjection;
using TwitterWebMVCv2.Data;
using TwitterWebMVCv2.Models;
using System;

namespace TwitterWebMVCv2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Wait to call .Run() on BuildWebHost until after stream is setup
            // or stream logic will not be called
            var host = BuildWebHost(args);
            
            // host.Run() must be called after creation of stream or stream will not be set up
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }


}

