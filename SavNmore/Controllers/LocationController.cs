using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using savnmore.Models;
using PagedList;
using savnmore.Services;
using System.Data.SqlServerCe;
namespace savnmore.Controllers
{
    public class LocationController : Controller
    {
        //
        // GET: /Loaction/
        private readonly savnmoreEntities _db = new savnmoreEntities();
        readonly ShoppingListService _sl = new ShoppingListService();
        public ActionResult Index(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.SortColumn = String.IsNullOrEmpty(sortColumn) ? "Zip" : sortColumn;
            ViewBag.CurrentSortOrder= String.IsNullOrEmpty(sortOrder) ? "" : sortOrder;
            ViewBag.NextSortOrder = String.IsNullOrEmpty(ViewBag.CurrentSortOrder) ? "desc" : "";
            string orderby = ViewBag.SortColumn + " " + ViewBag.CurrentSortOrder;
            orderby = orderby.Trim();
            if (Request.HttpMethod == "GET")
            {
                searchString = currentFilter;
            }
            else
            {
                page = 1;
            }
            ViewBag.CurrentFilter = searchString; 

            var zips = from t in _db.ZipCodes select t;
            if (!String.IsNullOrEmpty(searchString))
            {
                zips = zips.Where(s => s.ZipCode.ToUpper().Contains(searchString.ToUpper())
                                       || s.City.ToUpper().Contains(searchString.ToUpper()));
            }
            switch (orderby)
            {
                case "Zip desc":
                    zips = zips.OrderByDescending(s => s.ZipCode);
                    break;
                case "Latitude":
                    zips = zips.OrderBy(s => s.Latitude);
                    break;
                case "Latitude desc":
                    zips = zips.OrderByDescending(s => s.Latitude);
                    break;
                case "Longitude":
                    zips = zips.OrderBy(s => s.Longitude);
                    break;
                case "Longitude desc":
                    zips = zips.OrderByDescending(s => s.Longitude);
                    break;
                case "City":
                    zips = zips.OrderBy(s => s.City);
                    break;
                case "City desc":
                    zips = zips.OrderByDescending(s => s.City);
                    break;
                default:
                    zips = zips.OrderBy(s => s.ZipCode);
                    break;
            }
            int pageSize = 300;
            int pageNumber = (page ?? 1);

            return View(zips.ToPagedList(pageNumber, pageSize)); 

        }
        public JsonResult LoadZipCodes()
        {
            
            //open the file
            //read line by line
            try
            {

                foreach (var zipcode in _db.ZipCodes)
                {
                    _db.Entry(zipcode).State = EntityState.Deleted;

                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }

            FileStream fileStream = null;
            StreamReader streamReader = null;
            string conString = "Data Source=" + System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/Database1.sdf")+ ";Persist Security Info=False";//System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/Database1.sdf");
            string commandString = "Select * from ZipCodeEntries";  
             DataSet ds = new DataSet();

            //DataTable dt = new DataTable();
            //dt.Columns.Add(new DataColumn("ZipCode", Type.GetType("System.String")));
            //dt.Columns.Add(new DataColumn("Latitude", Type.GetType("System.Double")));
            //dt.Columns.Add(new DataColumn("Longitude", Type.GetType("System.Double")));
            //dt.Columns.Add(new DataColumn("City", Type.GetType("System.String")));
            //dt.Columns.Add(new DataColumn("SateId", Type.GetType("System.Integer")));
           
            using (SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(commandString, conString))
            {
                    dataAdapter.Fill(ds, "ZipCodeEntries");
            }
            try
            {
                fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/zip.csv"), FileMode.Open);
                streamReader = new StreamReader(fileStream);
                while (true)
                {
                    try
                    {
                        string line = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            break;
                        string[] zipdata = line.Split(',');
                       
                        var rw = ds.Tables["ZipCodeEntries"].NewRow();
                        rw[0] = zipdata[0];
                        rw[1] = Convert.ToDouble(zipdata[1]);
                        rw[2] = Convert.ToDouble(zipdata[2]);
                        rw[3] = zipdata[4];
                        rw[4] = Convert.ToInt32(zipdata[5]);
                        ds.Tables["ZipCodeEntries"].Rows.Add(rw);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }       
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
                if (fileStream != null)
                    fileStream.Close();
            }
            try
            {
                
                using (SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(commandString, conString))
                {
                    SqlCeCommandBuilder cmd = new SqlCeCommandBuilder(dataAdapter);
                   dataAdapter.Update(ds, "ZipCodeEntries");
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
            return Json("Loaded Zip codes.");
        }

        public ActionResult Distance()
        {
            MaxDistance md = new MaxDistance();
            //CookieHelper cookies = new CookieHelper();
            //var zip = cookies.GetZip();
            var zip = LocationService.GetUserZip();
            ViewBag.CurrentZipSearch =zip;
            return View(md);
        }

        [HttpPost]
        public ActionResult Distance(string zipsearch)
        {
            if (String.IsNullOrEmpty(zipsearch))
            {
                var zip = LocationService.GetUserZip();
                ViewBag.CurrentZipSearch = zip;
                return View(new MaxDistance());
            }
            LocationService.SaveZip(zipsearch);
            //CookieHelper cookies = new CookieHelper();
            //cookies.SetZip(zipsearch);
            LocationService ls = new LocationService();
            MaxDistance md = new MaxDistance();
            md.ZipCode = zipsearch;
            md.Miles = 5;
            md = ls.GetAllStores(md);
            LocationService.SaveZip(zipsearch);
            ViewBag.CurrentZipSearch = zipsearch;
            return View(md);
        }

        public ViewResult LocalSales()
        {
            //CookieHelper cookies = new CookieHelper();
            //ViewBag.CurrentZipSearch = cookies.GetZip();
            ViewBag.CurrentZipSearch = LocationService.GetUserZip();
            ViewBag.FindMiles = LocationService.GetMiles();
            return View();
        }
        public ViewResult FindAnItem()
        {
            //CookieHelper cookies = new CookieHelper();
            ViewBag.CurrentZipSearch = LocationService.GetUserZip();
            ViewBag.withinmiles = LocationService.GetMiles();
            return  View();
        }
        [HttpPost]
        public ActionResult FindAnItem(string searchterm, string withinmiles, string zipradius)
        {
            List<ItemSearchResult> searchResults = new List<ItemSearchResult>();
            //get the stores near the zip
            if (string.IsNullOrEmpty(searchterm) || string.IsNullOrEmpty(zipradius))
            {
                return PartialView("_ItemSearch", searchResults); 
            }
            LocationService ls = new LocationService();
            MaxDistance md = new MaxDistance();
            LocationService.SaveZip(zipradius);
            //CookieHelper cookies = new CookieHelper();
            //cookies.SetZip(zipradius); 
            md.ZipCode = zipradius;
            md.Miles = Convert.ToInt32(withinmiles);
            ViewBag.withinmiles = LocationService.GetMiles();
            ViewBag.SearchFor = searchterm;
            ViewBag.CurrentZipSearch = zipradius;
            md = ls.GetAllStores(md);
            //now find all items similar to the search term
            string searchfor = searchterm.ToLower();
           
            foreach (Store strs in md.CloseStores)
            {
                ItemSearchResult searchResult = new ItemSearchResult();
                searchResult.RelatedItems = new List<Item>();
                searchResult.Store = strs;
                searchResults.Add(searchResult);
            }
            DateTime cutoff = DateTime.Now.Date;
            foreach (var str in searchResults)
            {
                //see if this store has a weekly sale
                int id = str.Store.Id;               
                //and if that weekly sale contains an item with a similar name
                var wss = from t in _db.WeeklySales
                          where t.StoreId == id
                                && t.EndsOn >= cutoff
                          select t;
                //ws.AddRange(wss);
                foreach (var w in wss)
                {
                    var itms = from t in w.SaleItems
                               where t.Name.ToLower().Contains(searchfor)
                               select t;
                    str.RelatedItems.AddRange(itms.Select(t => new
                    Item
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        ImageUrl = t.ImageUrl,
                        Price = t.Price
                    }).Distinct());
                }
            }
            foreach (var itm in searchResults)
            {
                //see if this is on our list already
                itm.RelatedItems = itm.RelatedItems.OrderBy(t => t.Name).ToList();
                itm.RelatedItems = _sl.MarkWhatIsOnUsersShoppingList(itm.Store.Id, itm.RelatedItems);

            }
            return PartialView("_ItemSearch", searchResults); 
        }
        [HttpPost]
        public ActionResult FindStoresNear(string findmiles, string zipsearch)
        {
            ViewBag.CurrentZipSearch = zipsearch;
            //CookieHelper cookies = new CookieHelper();
            //cookies.SetZip(zipsearch); 
            LocationService.SaveZip(zipsearch);
            if (string.IsNullOrEmpty(zipsearch))
            {
                return View(new List<Store>());
            }
            LocationService ls = new LocationService();
            MaxDistance md = new MaxDistance();
            md.ZipCode = zipsearch;
            md.Miles = Convert.ToInt32(findmiles);
            ViewBag.FindMiles = LocationService.GetMiles();
            md = ls.GetAllStores(md);
            return View(md.CloseStores.ToList());
        }
        [HttpPost]
        public ActionResult FindSimilarItems(int itemId, int storeId, string searchFor, string miles)
        {
            
            //find all stores within 5 miles of this one
            LocationService ls = new LocationService();
            //get this store's address and zip code
            Store s = _db.Stores.Include("Address").Single(t => t.Id == storeId);
            Item i = _db.SaleItems.Single(t => t.Id == itemId);
            //split the items name up into spaces
            //string[] terms = i.Name.Split(' ');
            ////find the last string
            //string searchfor = terms[terms.Length-1];
            string searchfor = searchFor.ToLower();
            MaxDistance md = new MaxDistance();
            md.ZipCode = s.Address.Zip;
            md.Miles = Convert.ToInt32(miles);//20);
            var storeList = ls.FindStores(md);
            //if there are no stores, then no stores
            //for these stores
            //find all weekly sales items
            List<ItemSearchResult> searchResults = new List<ItemSearchResult>();
            foreach(Store strs in storeList)
            {
                if(strs.Id == storeId) //don't include this store, we are here already
                {
                    continue;                    
                }
                ItemSearchResult searchResult = new ItemSearchResult();
                searchResult.RelatedItems = new List<Item>();
                searchResult.Store = strs;
                searchResults.Add(searchResult);
            }
            DateTime cutoff = DateTime.Now.Date;
            foreach (var str in searchResults)
            {
                //see if this store has a weekly sale
                int id = str.Store.Id;
                
             
                //and if that weekly sale contains an item with a similar name
                var wss = from t in _db.WeeklySales
                         where t.StoreId == id
                               && t.EndsOn >= cutoff
                         select t;
                //ws.AddRange(wss);
                foreach (var w in wss)
                {
                    var itms = from t in w.SaleItems
                               where t.Name.ToLower().Contains(searchfor)
                               && t.Id != i.Id
                               select t;
                    str.RelatedItems.AddRange(itms.Select(t => new 
                    Item
                                                                   {
                                                                       Id = t.Id, 
                                                                       Name = t.Name,
                                                                       Description = t.Description,
                                                                       ImageUrl = t.ImageUrl,
                                                                       Price = t.Price
                                                                   }).Distinct());
                }
            }
            foreach (var itm in searchResults)
            {
                //see if this is on our list already
                itm.RelatedItems = itm.RelatedItems.OrderBy(t => t.Name).ToList();
                itm.RelatedItems = _sl.MarkWhatIsOnUsersShoppingList(itm.Store.Id, itm.RelatedItems);
                 
            }
          
            return PartialView("_ItemSearch", searchResults); 
        }
    }
}
