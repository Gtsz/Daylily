﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using Daylily.Bot.Attributes;
using Daylily.Bot.Enum;
using Daylily.Bot.Models;
using Daylily.Bot.PluginBase;
using Daylily.Common.Utils.LoggerUtils;
using Daylily.Common.Utils.RequestUtils;
using Daylily.CoolQ;
using Daylily.Osu.Database.BLL;
using Daylily.Osu.Database.Model;
using Daylily.Osu.Interface;
using Newtonsoft.Json;

namespace Daylily.Plugin.Osu
{
    [Name("Modding查询")]
    [Author("yf_extension")]
    [Version(0, 1, 0, PluginVersion.Beta)]
    [Help("查询Modding所得点赞或kd，并生成相应统计图。")]
    [Command("kd")]
    public class Kudosu : CommandPlugin
    {
        [Help("查询指定的osu用户名。若带空格，请使用引号。")]
        [FreeArg]
        public string OsuId { get; set; }

        public override void Initialize(string[] args)
        {

        }

        public override CommonMessageResponse Message_Received(CommonMessage messageObj)
        {
            string id;
            string uname;
            if (OsuId == null)
            {
                BllUserRole bllUserRole = new BllUserRole();
                List<TblUserRole> userInfo = bllUserRole.GetUserRoleByQq(long.Parse(messageObj.UserId));
                if (userInfo.Count == 0)
                    return new CommonMessageResponse(LoliReply.IdNotBound, messageObj, true);

                id = userInfo[0].UserId.ToString();
                uname = userInfo[0].CurrentUname;
            }
            else
            {
                int userNum = OldSiteApi.GetUser(OsuId, out var userObj);
                if (userNum == 0)
                    return new CommonMessageResponse(LoliReply.IdNotFound, messageObj, true);
                if (userNum > 1)
                {
                    // ignored
                }
                
                id = userObj.user_id;
                uname = userObj.username;
            }

            List<KudosuInfo> totalList = new List<KudosuInfo>();
            List<KudosuInfo> tmpList;
            int page = 0;
            const int count = 20;
            do
            {
                string json =
                    HttpClientUtil.HttpGet("https://osu.ppy.sh/users/" + id + "/kudosu?offset=" + page + "&limit=" +
                                           count);
                Logger.Debug("GET JSON");

                tmpList = JsonConvert.DeserializeObject<List<KudosuInfo>>(json);
                totalList.AddRange(tmpList);
                page += count;

                if (totalList.Count != 0) continue;

                return new CommonMessageResponse("此人一张图都没摸过……", messageObj, true);
            } while (tmpList.Count != 0);

            List<KdInfo> kdInfoList = new List<KdInfo>();
            int pastMonth = -1;
            KdInfo info = null;
            totalList.Reverse();
            foreach (var item in totalList)
            {
                if (item.Created_At.Month != pastMonth)
                {
                    if (pastMonth != -1)
                        kdInfoList.Add(info);
                    info = new KdInfo
                    {
                        Time = item.Created_At
                    };
                    pastMonth = item.Created_At.Month;
                }

                if (info != null) info.Count++;
            }

            if (info != null) kdInfoList.Add(info);

            var cqImg = new FileImage(Draw(kdInfoList, uname)).ToString();

            return new CommonMessageResponse(cqImg, messageObj);
        }

        private static Bitmap Draw(IReadOnlyList<KdInfo> kdInfoList, string uname)
        {
            int max = kdInfoList.Max(x => x.Count);
            string avg = "AVG: " + Math.Round(kdInfoList.Average(x => x.Count), 2);
            const int imgWidth = 430, imgHeight = 250;

            Bitmap bmp = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.FromArgb(34, 34, 34));
                const float maxHeight = imgHeight * 0.6F, maxWidth = imgWidth * 0.9F;
                float splitedWidth = maxWidth / (kdInfoList.Count - 1);
                const int splitedCount = 5;
                const float left = (imgWidth - maxWidth) / 2, top = (imgHeight - maxHeight) / 1.4F;
                RectangleF[] panels = { new RectangleF(left, top, maxWidth, maxHeight) };
                //max = (int)(max * 1.1);
                using (Brush yellowBrush = new SolidBrush(Color.FromArgb(255, 204, 34)))
                using (Brush whiteBrush = new SolidBrush(Color.FromArgb(224, 255, 255, 255)))
                using (Brush whiteBrush2 = new SolidBrush(Color.FromArgb(192, 255, 255, 255)))
                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                using (Pen yellowPen = new Pen(Color.FromArgb(255, 204, 34), 1))
                using (Pen yellowPen2 = new Pen(Color.FromArgb(128, 255, 204, 34)))
                using (Font f = new Font("Arial", 7, FontStyle.Italic))
                using (Font f2 = new Font("Arial", 7, FontStyle.Bold))
                using (Font f3 = new Font("Arial", 13, FontStyle.Italic | FontStyle.Bold))
                using (Font f4 = new Font("Arial", 9, FontStyle.Italic))
                {
                    SizeF sAvg = g.MeasureString(avg, f4);
                    SizeF sTitle = g.MeasureString(uname, f3);
                    g.DrawString(uname, f3, whiteBrush, imgWidth / 2f - sTitle.Width / 2f,
                        panels[0].Y - sTitle.Height - 30);
                    g.DrawString(avg, f4, whiteBrush, imgWidth / 2f - sAvg.Width / 2f, panels[0].Y - sAvg.Height - 14);

                    // g.DrawRectangles(blackPen, panels);
                    g.DrawLine(yellowPen2, panels[0].X, panels[0].Y, panels[0].X, panels[0].Y + panels[0].Height);
                    g.DrawLine(yellowPen2, panels[0].X, panels[0].Y + panels[0].Height, panels[0].X + panels[0].Width,
                        panels[0].Y + panels[0].Height);
                    const float circleWidth = 5;

                    int loopStep = (max - (max % splitedCount)) / splitedCount;
                    for (int i = 0; i <= splitedCount; i++)
                    {
                        string num = "";
                        float pTop = -1;
                        if (i == splitedCount)
                        {
                            pTop = top;
                            num = max.ToString();
                            if (max % splitedCount != 0)
                            {
                                float pTop2 = top + maxHeight * (max - i * loopStep) / max;
                                string num2 = (i * loopStep).ToString();
                                SizeF size2 = g.MeasureString(num2, f);
                                g.DrawString(num2, f, shadowBrush, left - size2.Width - 2 + 1,
                                    pTop2 - size2.Height / 2 + 1);
                                g.DrawString(num2, f, whiteBrush2, left - size2.Width - 2, pTop2 - size2.Height / 2);
                            }
                        }
                        else if (i != splitedCount)
                        {
                            pTop = top + maxHeight * (max - i * loopStep) / max;
                            num = (i * loopStep).ToString();
                        }

                        SizeF size = g.MeasureString(num, f);
                        g.DrawString(num, f, shadowBrush, left - size.Width - 2 + 1, pTop - size.Height / 2 + 1);
                        g.DrawString(num, f, whiteBrush2, left - size.Width - 2, pTop - size.Height / 2);
                    }

                    for (int i = 0; i < kdInfoList.Count - 1; i++)
                    {
                        float pTopNext = top + maxHeight * (max - kdInfoList[i + 1].Count) / max;
                        float pLeftNext = left + (i + 1) * splitedWidth;
                        g.DrawLine(yellowPen,
                            left + i * splitedWidth, top + maxHeight * (max - kdInfoList[i].Count) / max, pLeftNext,
                            pTopNext);
                    }

                    for (int i = 0; i < kdInfoList.Count; i++)
                    {
                        var cur = kdInfoList[i];
                        float x = left + i * splitedWidth, y = top + maxHeight * (max - cur.Count) / max;

                        string count = cur.Count.ToString();
                        SizeF size1 = g.MeasureString(count, f2);
                        g.DrawString(count, f2, shadowBrush, x - size1.Width / 2f + 1, y - size1.Height - 3 + 1);
                        g.DrawString(count, f2, whiteBrush2, x - size1.Width / 2f, y - size1.Height - 3);
                        g.FillEllipse(yellowBrush,
                            new RectangleF(x - circleWidth / 2, y - circleWidth / 2, circleWidth, circleWidth));

                        string mon = ConvertMonth(cur.Time);
                        SizeF size2 = g.MeasureString(mon, f);
                        g.DrawString(mon, f, shadowBrush, x - size2.Width / 2f + 1,
                            panels[0].Top + panels[0].Height + 5 + 1);
                        g.DrawString(mon, f, whiteBrush2, x - size2.Width / 2f, panels[0].Top + panels[0].Height + 5);
                    }
                }

            }

            return bmp;
        }

        private static string ConvertMonth(DateTime dt)
        {
            switch (dt.Month)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    return "";
            }
        }

        private class KudosuInfo
        {
            public int Id { get; set; }
            public string Action { get; set; }
            public int Amount { get; set; }
            public string Model { get; set; }
            public DateTime Created_At { get; set; }
            public Giver Giver { get; set; }
            public Post Post { get; set; }
            public Details Details { get; set; }
        }

        private class Giver
        {
            public string Url { get; set; }
            public string Username { get; set; }
        }

        private class Post
        {
            public string Url { get; set; }
            public string Title { get; set; }
        }

        private class Details
        {
            public string Event { get; set; }
        }

        private class KdInfo
        {
            public DateTime Time { get; set; }
            public int Count { get; set; }
        }
    }
}