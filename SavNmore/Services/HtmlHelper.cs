using System;
using System.Collections.Generic;

namespace savnmore.Services
{
    public class HtmlHelper
    {
        /// <summary>
        /// Finds the last index of the string or throws error
        /// </summary>
        /// <param name="theLine"></param>
        /// <param name="lookfor"></param>
        /// <returns></returns>
        public static string GetLastIndexOf(string theLine, string lookfor)
        {
            try
            {
                int index = theLine.LastIndexOf(lookfor);
                if (index >= 0)
                {
                    return theLine.Substring(index);
                }
                return theLine;
            }
            catch
            {
                throw;
            }
        }
        public static string GetValueBetween(string theLine, string startString, string endString)
        {
            string value = String.Empty;
            try
            {
                int startAt = theLine.IndexOf(startString);
                string substr = theLine.Substring(startAt + startString.Length);
                int endAt = substr.IndexOf(endString);
                if (endAt < 0)
                {
                    endAt = substr.Length;
                }
                value = substr.Substring(0, endAt);
            }
            catch (Exception)
            {
                throw;
            }
            return value;
        }
        /// <summary>
        /// Will replace any links in the text with the anchor's text
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string CleanLinks(string line)
        {
            int sindex = -1;
            do
            {
                sindex = line.LastIndexOf("<a");
                if (sindex >= 0)
                {
                    string link = GetValueBetween(line, "<a", "</a>");
                    string textOfLink = GetValueBetween(link, ">", "</a>");
                    line = line.Replace("<a" + link + "</a>", textOfLink);
                }
            }
            while (sindex > 0);
            return line;
        }
        public static List<string> GetAllLinks(string text)
        {
            List<string> myLinks = new List<string>();
            int sindx = -1;
            do
            {
                sindx = text.LastIndexOf("<a");
                if (sindx >= 0)
                {
                    string link = GetValueBetween(text, "<a", "</a");
                    if (string.IsNullOrEmpty(link))
                    {
                        myLinks.Add("<a" + link + "</a>");
                    }
                }
            }
            while (sindx > 0);
            return myLinks;
        }
        public static string GetValue(string theLine, string searchFor)
        {
            string linkValue = string.Empty;
            try
            {
                int startAt = theLine.IndexOf(searchFor);
                string substr = theLine.Substring(startAt);
                int endAt = substr.IndexOf("\"");
                linkValue = substr.Substring(0, endAt);
            }
            catch { }
            return linkValue;
        }
        public static string GetLinkValue(string theLine)
        {
            return GetValue(theLine, "http");
        }
        public static string CleanLine(string line)
        {
            string myLine = line.Replace("\t", "");
            myLine = myLine.Replace("&#39", "'");
            myLine = myLine.Replace("<br />", "");
            myLine = myLine.Replace("&quot;", "\"");
            myLine = myLine.Replace("&gt", ">");
            myLine = myLine.Replace("&lt", "<");
            myLine = myLine.Replace("&amp", "&");
            return myLine;
        }
        /// <summary>
        /// Returns the value inside the anchor 
        /// </summary>
        /// <param name="theLine"></param>
        /// <returns></returns>
        public static string GetAnchorText(string theLine)
        {
            string[] pieces = theLine.Split('>');
            if (pieces.Length > 1)
            {
                return pieces[1].Replace("</a", "");

            }
            return string.Empty;
        }
        public static string GetAttributeValue(string theLine, string attributeName)
        {
            string attValue;
            string tmpAttributeName = attributeName + "=\"";
            try
            {
                int startAt = theLine.IndexOf(tmpAttributeName);
                string substr = theLine.Substring(startAt + tmpAttributeName.Length);
                int endAt = substr.IndexOf("\"");
                attValue = substr.Substring(0, endAt);
            }
            catch
            {
                throw;
            }
            return attValue;
        }
        public static string GetImageSource(string theLine)
        {
            string attValue;
            const string tmpAttributeName = "src='";
            try
            {
                int startAt = theLine.IndexOf(tmpAttributeName, StringComparison.Ordinal);
                string substr = theLine.Substring(startAt + tmpAttributeName.Length);
                int endAt = substr.IndexOf("'",StringComparison.Ordinal);
                attValue = substr.Substring(0, endAt);
            }
            catch
            {
                attValue = string.Empty;
            }
            return attValue;
        }
        public static bool IsValueEmpty(string value)
        {
            return value.Length == 0;
        }
    }
}
