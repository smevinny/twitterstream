using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Emitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using TwitterStream.Helpers;
using TwitterStream.Models;
using Microsoft.Extensions.Options;

namespace TwitterStream.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwitterStreamingController 
    {
        private readonly ITwitterCredentials _credentials;
        public string _filePath;

        public TwitterStreamingController(IOptions<AppSettings> appSettings)
        {
            _credentials = new TwitterCredentials(appSettings.Value.ConsumerKey, appSettings.Value.ConsumerSecret, appSettings.Value.AccessToken, appSettings.Value.AccessTokenSecret);
            _filePath = appSettings.Value.AnalyticFilePath;
        }

        // start twitter streaming service
        [HttpGet]
        [Route("Start")]
        public async Task Start()
        {
            var client = new TwitterClient(_credentials);
            // Using the sample stream
            var stream = client.Streams.CreateSampleStream();

            try
            {
                //setting stream filter levels
                stream.AddLanguageFilter(LanguageFilter.English);
                stream.FilterLevel = Tweetinvi.Streaming.Parameters.StreamFilterLevel.Low;

                DateTime streamingStartedOn = DateTime.UtcNow;
                TwitterData _data = new TwitterData();

                //set file for save twitter data
                if (File.Exists(_filePath))
                    File.Delete(_filePath);

                File.Create(_filePath).Dispose();

                stream.TweetReceived += (sender, t) =>
                {
                    //exclude retweets
                    if (t.Tweet.IsRetweet)
                        return;

                    //setting total tweets
                    _data.TotalTweets++;

                    //setting average tweet per hour / minute / second
                    var timeDiff = DateTime.UtcNow - streamingStartedOn;
                    _data.AverageTweetPerHour = Convert.ToInt32(_data.TotalTweets / timeDiff.TotalHours);
                    _data.AverageTweetPerMinute = Convert.ToInt32(_data.TotalTweets / timeDiff.TotalMinutes);
                    _data.AverageTweetPerSecond = Convert.ToInt32(_data.TotalTweets / timeDiff.TotalSeconds);

                    if (_data.TotalTweets == 1)
                    {
                        _data.TopDomains = new List<string>();
                        _data.TopHashtags = new List<string>();
                        _data.TopEmojis = new List<string>();
                    }

                    //setting top domains and url percentage
                    if (t.Tweet.Urls.Count > 0)
                    {
                        _data.TotalTweetWithUrl++;
                        _data.TopDomains.AddRange(t.Tweet.Urls.Select(u => u.DisplayedURL.Split("/")[0]).Distinct().ToList());
                    }
                    _data.UrlPercentage = Math.Round((decimal)(_data.TotalTweetWithUrl * 100 / _data.TotalTweets), 2);

                    //setting photo percentage
                    if (t.Tweet.Entities.Medias.Count > 0)
                    {
                        _data.TotalTweetWithPhoto++;
                    }
                    _data.PhotoPercentage = Math.Round((decimal)(_data.TotalTweetWithPhoto * 100 / _data.TotalTweets), 2);

                    //setting top emoji and emoji percentage
                    var emojis = t.Tweet.FullText.Where(c => c > 255).Select(emoji => Utils.Utf8Encoder.GetString(Utils.Utf8Encoder.GetBytes(emoji.ToString()))).Distinct().ToList();
                    if (emojis != null)
                    {
                        if (emojis.Count > 0)
                        {
                            _data.TotalTweetWithEmoji++;
                            _data.TopEmojis.AddRange(emojis);
                            //_data.TopEmojis.AddRange(t.Tweet.Entities.Symbols.Select(s => s.Text).Distinct().ToList());
                        }
                        _data.EmojiPercentage = Math.Round((decimal)(_data.TotalTweetWithEmoji * 100 / _data.TotalTweets), 2);
                    }

                    //setting top hashtags
                    if (t.Tweet.Hashtags.Count > 0)
                    {
                        _data.TopHashtags.AddRange(t.Tweet.Hashtags.Select(h => h.Text).Distinct().ToList());
                    }

                    //group and order by numbers
                    _data.TopDomains = _data.TopDomains.GroupBy(domain => domain).OrderByDescending(domain => domain.Count()).Select(domain => domain.Key).Take(5).ToList();
                    _data.TopHashtags = _data.TopHashtags.GroupBy(hash => hash).OrderByDescending(hash => hash.Count()).Select(hash => hash.Key).Take(5).ToList();
                    _data.TopEmojis = _data.TopEmojis.GroupBy(emoji => emoji).OrderByDescending(emoji => emoji.Count()).Select(emoji => emoji.Key).Take(5).ToList();

                    string json = JsonConvert.SerializeObject(_data);

                    if (File.Exists(_filePath))
                    {
                        //write data in file
                        File.WriteAllText(_filePath, json);
                    }
                    else
                    {
                        //stop streaming service
                        stream.Stop();
                    }
                };

                // Start
                await stream.StartAsync();
            }
            catch(Exception ex)
            {
                //stop streaming service on exception
                stream.Stop();
                throw ex;
            }
        }

        // stop twitter streaming service
        [HttpGet]
        [Route("Stop")]
        public bool Stop()
        {
            //delete existing data file
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            return true;
        }

    }
}
