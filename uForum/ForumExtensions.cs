﻿using HtmlAgilityPack;
using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using uForum.Library;
using uForum.Models;
using uForum.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace uForum
{
    public static class ForumExtensions
    {
        
        public static IEnumerable<Comment> ChildComments(this Comment comment)
        {
            if (comment.HasChildren)
            {
                using (var cs = new CommentService())
                {
                    return cs.GetChildComments(comment.Id);
                }
            }

            return new List<Comment>();
        }


        public static string ConvertToRelativeTime(this DateTime date)
        {
            
            var TS = DateTime.Now.Subtract(date);
            var span = int.Parse(Math.Round(TS.TotalSeconds, 0).ToString());

            if (span < 60)
                return "1 minute ago";

            if (span >= 60 && span < 3600)
                return string.Concat(Math.Round(TS.TotalMinutes), " minutes ago");

            if (span >= 3600 && span < 7200)
                return "1 hour ago";

            if (span >= 3600 && span < 86400)
                return string.Concat(Math.Round(TS.TotalHours), " hours ago");

            if (span >= 86400 && span < 172800)
                return "1 day ago";

            if (span >= 172800 && span < 604800)
                return string.Concat(Math.Round(TS.TotalDays), " days ago");

            if (span >= 604800 && span < 1209600)
                return "1 week ago";

            if (span >= 1209600 && span < 2592000)
                return string.Concat(Math.Round(TS.TotalDays), " days ago");

            if (span >= 2592000 && span < 26920000)
                return "Several months ago";

            return "More then a year ago";
        }


        public static HtmlString Sanitize(this string html){
            // Run it through Markdown first
            var md = new Markdown();
            html = md.Transform(html);

            // Linkify images if they are shown as resized versions (only relevant for new Markdown comments)
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var root = doc.DocumentNode;
            if (root != null)
            {
                var images = root.SelectNodes("//img");
                if (images != null)
                {
                    var replace = false;
                    foreach (var image in images)
                    {
                        var src = image.GetAttributeValue("src", "");
                        var orgSrc = src.Replace("rs/", "");

                        if (src == orgSrc || image.ParentNode.Name == "a") continue;

                        var a = doc.CreateElement("a");
                        a.SetAttributeValue("href", orgSrc);
                        a.SetAttributeValue("target", "_blank");

                        a.AppendChild(image.Clone());

                        image.ParentNode.ReplaceChild(a, image);

                        replace = true;
                    }

                    if (replace)
                    {
                        html = root.OuterHtml;
                    }
                }
            }

            return new HtmlString(Utills.Sanitize(html));
        }

        public static bool DetectSpam(this Comment comment)
        {
            comment.IsSpam = AntiSpam.SpamChecker.IsSpam(comment.Author(), comment.Body, "comment", comment.TopicId);
            return comment.IsSpam;
        }

        public static bool DetectSpam(this Topic topic)
        {
            topic.IsSpam = AntiSpam.SpamChecker.IsSpam(topic.Author(), topic.Body, "comment", topic.Id);
            return topic.IsSpam;
        }
    }
}
