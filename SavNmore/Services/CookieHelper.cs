using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace savnmore.Services
{
    public class CookieHelper
    {
        public static string CookieName { get; set; }
        public const string Zip = "zip";
        public CookieHelper()
        {
            CookieName = "savnmorecom";
        }
        public string GetZip()
        {
            
            return GetCookieValue(Zip);
            
        }
        public void SetZip(string zip)
        { 
            UpdateCookie(Zip, zip); 
        }
        public void UpdateCookie(string key, string value)
        {
            var myCookie = GetCookie();
            myCookie[key] = value;
            HttpContext.Current.Request.Cookies.Set(myCookie);
        }
        public HttpCookie GetCookie()
        {
            if (HttpContext.Current.Request.Cookies[CookieName] == null)
            {
                HttpCookie myCookie = new HttpCookie(CookieName);
                myCookie.Expires = DateTime.Now.AddDays(30);
                HttpContext.Current.Request.Cookies.Add(myCookie);
                return myCookie;
            }
            var cookie = HttpContext.Current.Request.Cookies[CookieName];
            if (cookie.Expires <= DateTime.Now)
            {
                cookie.Expires = DateTime.Now.AddDays(30);
                HttpContext.Current.Request.Cookies.Set(cookie);
            }
            return cookie;
        }
        public string GetCookieValue(string key)
        {
            var myCookie = GetCookie();
            try
            {
                return myCookie[key];
            }
            catch
            {
                return string.Empty;
            }
           
        }
    }

}