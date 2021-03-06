﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.Osu.ApiV2b;
using Bleatingsheep.Osu.ApiV2b.Models;

namespace Daylily.Osu.Interface
{
    public static class NewSiteApi
    {
        public static async Task<Beatmapset> GetBeatmapsetsBySidAsync(string sId)
        {
            OsuApiV2Client client = new OsuApiV2Client(OsuApiKey.UserName, OsuApiKey.Password);
            try
            {
                Beatmapset set = await client.GetBeatmapsetBySIdAsync(sId);
                return set;
            }
            catch (NetworkFailException e)
            {
                if (e.InnerException.Message.Contains("404"))
                    return null;
                throw;
            }
        }
        public static async Task<Beatmapset> GetBeatmapsetsByBidAsync(string bId)
        {
            OsuApiV2Client client = new OsuApiV2Client(OsuApiKey.UserName, OsuApiKey.Password);
            try
            {
                Beatmapset set = await client.GetBeatmapsetByBIdAsync(bId);
                return set;
            }
            catch (NetworkFailException e)
            {
                if (e.InnerException.Message.Contains("404"))
                    return null;
                throw;
            }
        }

        public static Beatmapset GetBeatmapsetsBySid(string sId)
        {
            OsuApiV2Client client = new OsuApiV2Client(OsuApiKey.UserName, OsuApiKey.Password);
            return client.GetBeatmapsetBySIdAsync(sId).Result;
        }
        public static Beatmapset GetBeatmapsetsByBid(string bId)
        {
            OsuApiV2Client client = new OsuApiV2Client(OsuApiKey.UserName, OsuApiKey.Password);
            return client.GetBeatmapsetByBIdAsync(bId).Result;
        }
        public static Beatmapset[] SearchBeatmaps(string keyword, BeatmapsetsSearchOptions options = null)
        {
            OsuApiV2Client client = new OsuApiV2Client(OsuApiKey.UserName, OsuApiKey.Password);
            return client.SearchBeatMapAsync(keyword, options).Result.Beatmapsets;
        }

        public static Beatmapset[] SearchAllBeatmaps(string keyword, BeatmapsetsSearchOptions options = null)
        {
            OsuApiV2Client client = new OsuApiV2Client(OsuApiKey.UserName, OsuApiKey.Password);
            List<Beatmapset> list = new List<Beatmapset>();
            Beatmapset[] tmpArray;
            int i = 1;
            do
            {
                //Console.WriteLine($"page {i}.");
                var newOptions = new BeatmapsetsSearchOptions
                {
                    Page = i
                };

                if (options != null)
                {
                    newOptions.Extra = options.Extra;
                    newOptions.Genre = options.Genre;
                    newOptions.Language = options.Language;
                    newOptions.Mode = options.Mode;
                    newOptions.Status = options.Status;
                }

                tmpArray = client.SearchBeatMapAsync(keyword, newOptions).Result.Beatmapsets;
                if (tmpArray.Length > 0)
                    list.AddRange(tmpArray);
                else
                {
                    //Console.WriteLine($"page {i} no result.");
                }

                i++;
            } while (tmpArray.Length > 0);

            return list.ToArray();
        }
    }
}
