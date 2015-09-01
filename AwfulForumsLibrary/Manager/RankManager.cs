using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class RankManager
    {
        private readonly IWebManager _webManager;

        public RankManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public RankManager() : this(new WebManager())
        {
        }

        public async Task<List<MostStartedThreadRankEntity>> GetMostStartedThreadsRank(int forumId = 1)
        {
            try
            {
                HtmlDocument doc = (await _webManager.GetData(string.Format(Constants.StatsSpecificBase, 6, forumId))).Document;

                var statNodes =
                    doc.DocumentNode.Descendants("table")
                        .Where(node => node.GetAttributeValue("class", string.Empty).Contains("stats"));

                var threads = new List<MostStartedThreadRankEntity>();
                foreach (var node in statNodes)
                {
                    node.Descendants("tr").FirstOrDefault().Remove();
                    var data = node.Descendants("tr").ToList();
                    foreach (var row in data)
                    {
                        var item = new MostStartedThreadRankEntity();
                        item.FromTable(row.Descendants("td").ToList());
                    }
                }

                return threads;

            }
            catch (Exception ex)
            {
                return new List<MostStartedThreadRankEntity>(0);
            }
        }

        public async Task<List<MostPostsPerHourRankEntity>> GetMostPostsPerHourRank(int forumId = 1)
        {
            try
            {
                HtmlDocument doc = (await _webManager.GetData(string.Format(Constants.StatsSpecificBase, 7, forumId))).Document;

                var statNodes =
                    doc.DocumentNode.Descendants("table")
                        .Where(node => node.GetAttributeValue("class", string.Empty).Contains("stats"));

                var threads = new List<MostPostsPerHourRankEntity>();
                foreach (var node in statNodes)
                {
                    node.Descendants("tr").FirstOrDefault().Remove();
                    var data = node.Descendants("tr").ToList();
                    foreach (var row in data)
                    {
                        var item = new MostPostsPerHourRankEntity();
                        item.FromTable(row.Descendants("td").ToList());
                    }
                }

                return threads;

            }
            catch (Exception ex)
            {
                return new List<MostPostsPerHourRankEntity>(0);
            }
        }

        public async Task<List<GlobalRankEntity>> GetIgnoredUsersRank()
        {
            return await GetGlobalStats(string.Format(Constants.StatsBase, 4));
        }

        public async Task<List<GlobalRankEntity>> GetHighestRatedThreadsRank()
        {
            return await GetGlobalStats(string.Format(Constants.StatsBase, 8));
        }

        public async Task<List<GlobalRankEntity>> GetBuddyListedUsersRank()
        {
            return await GetGlobalStats(string.Format(Constants.StatsBase, 3));
        }

        public async Task<List<GlobalRankEntity>> GetGassedThreadRank()
        {
            return await GetGlobalStats(string.Format(Constants.StatsBase, 2));
        }

        public async Task<List<GlobalRankEntity>> GetBansPerAdminRank()
        {
            return await GetGlobalStats(string.Format(Constants.StatsBase, 9));
        }

        public async Task<List<GlobalRankEntity>> GetGoldmindedThreadRank()
        {
            return await GetGlobalStats(string.Format(Constants.StatsBase, 1));
        }

        private async Task<List<GlobalRankEntity>> GetGlobalStats(string url)
        {
            try
            {
                HtmlDocument doc = (await _webManager.GetData(url)).Document;

                var statNodes =
                    doc.DocumentNode.Descendants("table")
                        .Where(node => node.GetAttributeValue("class", string.Empty).Contains("stats"));

                var threads = new List<GlobalRankEntity>();
                foreach (var node in statNodes)
                {
                    node.Descendants("tr").FirstOrDefault().Remove();
                    var data = node.Descendants("tr").ToList();
                    foreach (var row in data)
                    {
                        var item = new GlobalRankEntity();
                        item.FromTable(row.Descendants("td").ToList());
                    }
                }

                return threads;

            }
            catch (Exception ex)
            {
                return new List<GlobalRankEntity>(0);
            }
        }
    }
}
