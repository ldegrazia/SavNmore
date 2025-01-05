using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using PagedList;
using savnmore.Models;
using savnmore.Services;

namespace savnmore.Controllers
{ 
    public class ItemsController : Controller
    {
        private readonly savnmoreEntities _db = new savnmoreEntities();
        readonly ShoppingListService _sl = new ShoppingListService();
        //
        // GET: /Items/

        public ViewResult Index(int id,int? page,string currentFilter, string searchString , string sortOrder)
        {
            WeeklySale weeklysale = _db.WeeklySales.Find(id);
            var store = GetStore(weeklysale);
            ViewBag.Store = store;
            ViewBag.WeeklySale = weeklysale;
            if (Request.HttpMethod == "GET")
            {
                searchString = currentFilter;
            }
            else
            {
                page = 1;
            }
            
         
            ViewBag.CurrentFilter = searchString;
            //update the list of the user's items
            var itms = from t in weeklysale.SaleItems select t;//RemoveFromList( true ) weeklysale.SaleItems.OrderBy(t => t.Name);
            ViewBag.TotalItems = weeklysale.SaleItems.Count();
            if (!String.IsNullOrEmpty(searchString))
            {
                itms = itms.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper())
                                       || s.Description.ToUpper().Contains(searchString.ToUpper()));
               
            }
            if(String.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "AtoZ";
            }
            //if the session is not null
            //compare with sortOrder
            ViewBag.CurrentSort = sortOrder;
            var orderedby = SortingService.SortItems(itms,sortOrder); //itms.OrderBy(t => t.Name).ToList();//order here
            orderedby = _sl.MarkWhatIsOnUsersShoppingList(store.Id, orderedby);
            if (!String.IsNullOrEmpty(searchString))
            {
                ViewBag.TotalItems = orderedby.Count();
            }
            int pageSize = 60; //move to constant
            int pageNumber = (page ?? 1);

            ViewBag.SortOptions = SortingService.GetSortOptions(sortOrder);
            return View(orderedby.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Items/Details/5

        public ViewResult Details(int id)
        {
            Item item = _db.SaleItems.Find(id);
            var ws = GetWeeklySale(item);
            ViewBag.WeeklySaleId = ws.Id;
            ViewBag.StoreId = ws.StoreId;
            ViewBag.StoreName = ws.Store.Name;
            //is this item on the shopping list?
            item.OnList = _sl.IsItemOnList(ws.StoreId, item.Id);
            //need store id
            string[] terms = item.Name.Split(' ');
            if (terms.Length > 2)
            {
                //find the last strings
                ViewBag.SearchFor = terms[terms.Length - 2] + " " + terms[terms.Length - 1];
            }
            else
            {
                ViewBag.SearchFor = item.Name;
            }
            //Get the miles
            ViewBag.Miles = LocationService.GetMiles();
            return View(item);
        }

        //
        // GET: /Items/Create
        

       
        
        public ActionResult Create(int id)
        {
            ViewBag.WeeklySaleId = id;
            return View();
        } 

        //
        // POST: /Items/Create

        [HttpPost]
        public ActionResult Create(Item item, int weeklysaleid)
        {
            ViewBag.WeeklySaleId = weeklysaleid;
            if (ModelState.IsValid)
            {
                WeeklySale ws = _db.WeeklySales.Find(weeklysaleid);
                if(ws.SaleItems == null)
                {
                    ws.SaleItems = new List<Item>();
                }
                ws.SaleItems.Add(item);
                //db.SaleItems.Add(item);
                _db.SaveChanges();
                int id = weeklysaleid;
                return RedirectToAction("Index", new{id});  
            }
            return View(item);
        }
        
        //
        // GET: /Items/Edit/5
 
        public ActionResult Edit(int id)
        {
            Item item = _db.SaleItems.Find(id);
            ViewBag.WeeklySaleId = GetWeeklySaleId(item);
            return View(item);
        }

        //
        // POST: /Items/Edit/5

        [HttpPost]
        public ActionResult Edit(Item item)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();
                int id = GetWeeklySaleId(item);
                return RedirectToAction("Index", new {id});
            }
            return View(item);
        }

        //
        // GET: /Items/Delete/5
 
        public ActionResult Delete(int id)
        {
            Item item = _db.SaleItems.Find(id);
            ViewBag.WeeklySaleId = GetWeeklySaleId(item);
            return View(item);
        }

        //
        // POST: /Items/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Item item = _db.SaleItems.Find(id);
            int weeklySaleId =  GetWeeklySaleId(item);
            _db.SaleItems.Remove(item);
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = weeklySaleId });
        }

        public int GetWeeklySaleId(Item item)
        {
            return _db.WeeklySales.Single(i => i.SaleItems.Any(p => p.Id == item.Id)).Id;
        }
        public WeeklySale GetWeeklySale(Item item)
        {
            return _db.WeeklySales.Single(i => i.SaleItems.Any(p => p.Id == item.Id));
        }
        public Store GetStore(WeeklySale ws)
        {
            return _db.Stores.Single(i => i.WeeklySales.Any(p => p.Id == ws.Id));
        }
        public JsonResult AddToList(int storeId,int itemId)
        {
            ShoppingListService srvc = new ShoppingListService();
            srvc.AddItem(storeId,itemId);
            return Json(itemId);
        }
        public JsonResult Add1ItemUpdateTotal(int storeId, int itemId)
        {
            ShoppingListService srvc = new ShoppingListService();
            var newTotal = srvc.AddItemUpdateTotal(storeId, itemId);
            return Json(newTotal);
        }
        public JsonResult Remove1ItemUpdateTotal(int storeId, int itemId)
        {
            ShoppingListService srvc = new ShoppingListService();
            var newTotal = srvc.Remove1ItemUpdateTotal(storeId, itemId);
            return Json(newTotal);
        }
        //
        public JsonResult RemoveFromList(int storeId, int itemId)
        {
            ShoppingListService srvc = new ShoppingListService();
            var newTotal = srvc.RemoveItem(storeId, itemId);
            return Json(newTotal);
        }
        public JsonResult GetTotal(int storeId)
        {
            ShoppingListService srvc = new ShoppingListService();
            var newTotal = srvc.GetTotal(storeId);
            return Json(newTotal);
        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
      }
}