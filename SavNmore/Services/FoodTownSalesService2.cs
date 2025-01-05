using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using savnmore.Models;

namespace savnmore.Services
{
    public class FoodTownSalesService : IWeeklySaleService
    {
        private HttpWebRequest _request = null;
        private HttpWebResponse _response = null;

        public Models.WeeklySale GetWeeklySale(Models.Store store)
        {
            WeeklySale ws = new WeeklySale();
            _request = null;
            _response = null;
            List<CircularPage> pages = new List<CircularPage>();
            try
            {
                _request =
                    (HttpWebRequest)
                    WebRequest.Create(
                        new Uri(store.Url, UriKind.RelativeOrAbsolute));
                _response = (HttpWebResponse) _request.GetResponse();
                using (Stream responseStream = _response.GetResponseStream())
                {
                    using (StreamReader htmlStream = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string line;
                        LoopController saleLoop = new LoopController(true);
                        while (!saleLoop.IsStopped)
                        {
                            line = htmlStream.ReadLine();
                           
                            if (line == null || line.Contains("Footer"))
                            {
                                saleLoop.Stop();
                                continue;
                            }
                            if (line.Contains("CircularValidDates"))
                            {
                                //get the prices
                                string pricesgood = HtmlHelper.GetValueBetween(line, "good ", "</p>");
                                ws = GetDates(ws, pricesgood);
                                continue;
                            }
                            if (line.Contains("class=\"CircularImageBox\""))//basically, if 
                            //the next week circular is available then do this...
                            {
                                //we need to get the link for this page
                                CircularPage cp = new CircularPage();
                                cp.PageNumber = "1";
                                cp.Url = HtmlHelper.GetValueBetween(line, "href=\"", "\"><img ");
                                pages.Add(cp);
                                //now we must get the circular pages
                                pages = GetPagesFromFirst(pages);
                                //stop the looping
                                saleLoop.Stop();
                                 
                            }
                            if (line.Contains("mainPageCircList"))
                            {
                                //get the circular pages here
                                pages = GetCircularPages(htmlStream);
                                //saleLoop.Stop();
                                foreach (var cp in pages)
                                {
                                    System.Diagnostics.Debug.WriteLine(cp.PageNumber + " " + cp.Url);
                                }
                            }
                        }
                        //loop over for the dates
                    } //end using
                } //endusing
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            //now loop over very page and get the items
            ws.SaleItems = new List<Item>();
            ws.SaleItems = GetItems(pages);
            return ws;
        }

        private WeeklySale GetDates(WeeklySale ws, string theDates)
        {
            try
            {
                string[] currentstring = theDates.Split(' ');
                ws.StartsOn = DateTime.Parse(currentstring[0]);
                ws.EndsOn = DateTime.Parse(currentstring[2]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Cant parse dates:" + ex.Message);
            }
            return ws;
        }

        private List<CircularPage> GetPagesFromFirst(List<CircularPage> pages)
        {
            //hit the first page
            CircularPage fp = pages[0];
            try
            {
                _request =
                    (HttpWebRequest)
                    WebRequest.Create(
                        new Uri(fp.Url, UriKind.RelativeOrAbsolute));
                _response = (HttpWebResponse) _request.GetResponse();
                using (Stream responseStream = _response.GetResponseStream())
                {
                    using (StreamReader htmlStream = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string line;
                        LoopController pageLoop = new LoopController(true);
                        while (!pageLoop.IsStopped)
                        {
                            line = htmlStream.ReadLine();
                            if (line == null || line.Contains("circ_instruction"))
                            {
                                pageLoop.Stop();
                                continue;
                            }
                            if (line.Contains("class=\"nav\""))
                            {
                                try
                                {
                                    CircularPage cp = new CircularPage();
                                    cp.PageNumber = HtmlHelper.GetAnchorText(line);
                                    int validint;
                                    if(!Int32.TryParse(cp.PageNumber,out validint))
                                    {
                                        //skip it
                                        continue;
                                    }
                                    cp.Url = HtmlHelper.GetValueBetween(line, "href=\"", "\" class=");
                                    pages.Add(cp);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("Error getting circular page:" + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return pages;
        }

        private List<CircularPage> GetCircularPages(StreamReader htmlStream)
        {
            List<CircularPage> pages = new List<CircularPage>();
            //read the stream for each page
            //loop over the stream until we get to <div>
            string line;
            LoopController circularloop = new LoopController(true);

            while (!circularloop.IsStopped)
            {
                line = htmlStream.ReadLine();
                if (line == null || line.Contains("circ_instruction"))
                {
                    circularloop.Stop();
                    continue;
                }
                if (line.Contains("class=\"nav\""))
                {
                    try
                    {
                        CircularPage cp = new CircularPage();
                        cp.PageNumber = HtmlHelper.GetAnchorText(line);
                        
                        cp.Url = HtmlHelper.GetValueBetween(line, "href=\"", "\" class=");
                        pages.Add(cp);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error getting circular page:" + ex.Message);
                    }
                }
            }
            return pages;
        }

        private List<Item> GetItems(List<CircularPage> pages)
        {
            List<Item> saleItems = new List<Item>();
            //foreach page
            foreach (CircularPage cp in pages)
            {  //hit the page
                    //get the items
                    //return the items
                try
                {
                    _request = (HttpWebRequest) WebRequest.Create(new Uri(cp.Url, UriKind.RelativeOrAbsolute));
                    _response = (HttpWebResponse) _request.GetResponse();
                    using (Stream responseStream = _response.GetResponseStream())
                    {
                        using (StreamReader htmlStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            string line;
                            LoopController saleLoop = new LoopController(true);
                            while (!saleLoop.IsStopped)
                            {
                                line = htmlStream.ReadLine();
                                if (line == null || line.Contains("frmCircPage"))
                                {
                                    saleLoop.Stop();
                                    continue;
                                }
                                if(line.Contains("class=\"img\""))
                                {
                                    try
                                    {
                                        var item = GetItemDetails(htmlStream,line);
                                        saleItems.Add(item);
                                    }
                                    catch 
                                    {
                                        //gulp
                                    }
                                }
                            } //end while
                        }
                    }

                  
                } //end try
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            saleItems.Sort();
            IEqualityComparer<Item> customComparer = new ItemComparer();
            IEnumerable<Item> distinctItems = saleItems.Distinct(customComparer);
            return distinctItems.ToList();
            
        }
        private Item GetItem(string line)
        {
            Item i = new Item();
            i.ImageUrl = HtmlHelper.GetValueBetween(line, "src=\"", "\" class=\"img\" />");
            i.Name = HtmlHelper.GetValueBetween(line, "title\">", "</p>");
            i.Description = HtmlHelper.GetValueBetween(line, "desc\">", "</p>");
            i.Price = HtmlHelper.GetValueBetween(line, "price\">", "</p>");
            return i;
        }
        private Item GetItemDetails(StreamReader htmlStream, string line)
        {
            Item i = new Item();
             LoopController itemLoop = new LoopController(true);
            do
            {
                    if (line == null)
                    {
                    itemLoop.Stop();
                    }
                    if(line != null && line.Contains("src"))
                    {
                          i.ImageUrl = HtmlHelper.GetValueBetween(line, "src=\"", "\" class=\"img\" />");
                    }
                    if (line.Contains("title\">"))
                    {
                        i.Name = HtmlHelper.GetValueBetween(line, "title\">", "</p>");
                    }
                    if (line.Contains("desc\">"))
                    {
                        //we need to get the description
                        if(!line.Contains("</p><p class=\"price\">"))//if the line has ending </p> then we are good
                        {
                            var desc = line;
                            do
                            {
                                line = htmlStream.ReadLine();
                                desc += " " + line;
                             // if not we need to apend until we get to </p>
                            } while (!line.Contains("</p><p class=\"price\">"));
                            i.Description = HtmlHelper.GetValueBetween(desc, "desc\">", "</p>");
                        }
                        else
                        {
                             i.Description = HtmlHelper.GetValueBetween(line, "desc\">", "</p>"); 
                        }
                    }
                   
                    if(line.Contains("price\">"))
                    {
                        i.Price = HtmlHelper.GetValueBetween(line, "price\">", "</p>");
                        //try to convert to a double
                        try
                        {
                            i.SalePrice = GetSalePrice(i.Price);
                        }
                        catch (Exception ex)
                        {

                            System.Diagnostics.Debug.WriteLine("Could not get saleprice " + ex.Message);
                        }
                        
                    }
                    if ( line.Contains("</div>"))
                    {
                        itemLoop.Stop();
                    }
                    else
                    {
                         line = htmlStream.ReadLine();
                    }
               
            } while (!itemLoop.IsStopped);
            //keep reading the line for the div
            if (i.Description.Length > 500)
            {
                i.Description = i.Description.Substring(0, 500);
            }
            return i;
        }
        public static double? GetSalePrice(string priceText)
        {
            double price = 0;
            
            if(string.IsNullOrEmpty(priceText))
            {
                return null;
            }
            if(priceText.Contains("$"))
            {
                string[] priceElements = priceText.Split(' ');
                if(priceElements.Length == 0)
                {
                    if (!double.TryParse(priceText, out price))
                        {
                            return null; //no price
                        }
                    else
                    {
                        return price;
                    }
                }
                foreach(string p in priceElements)
                {
                    if(p.Contains("$"))
                    {
                        //split on $
                        string s = p.Replace('$',' ');
                        s = s.Trim();
                        if (!double.TryParse(s, out price))
                        {
                            return null; //no price
                        }
                        else
                        {
                            return price;
                        }

                    }
                }

            }
            if(priceText.Contains("¢"))
            {
                //its usually at the end
                string[] priceElements = priceText.Split(' ');
                if (priceElements.Length == 0)
                {
                    string s = priceText.Replace('¢', ' ');
                    s = s.Trim();
                    if (!double.TryParse(priceText, out price))
                    {
                        return null; //no price
                    }
                    else
                    {
                        return price;
                    }
                }
                foreach (string p in priceElements)
                {
                    if (p.Contains("¢"))
                    {
                        string s = priceText.Replace('¢', ' ');
                        s = s.Trim();
                        if (!double.TryParse(s, out price))
                        {
                            return null; //no price
                        }
                        else
                        {
                            return price/100;
                        }

                    }
                }
            }
            return price;
        }
    }
    
    public class CircularPage
    {
        public string PageNumber { get; set; }
        public string Url { get; set; }
    }
}