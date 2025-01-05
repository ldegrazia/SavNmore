using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using savnmore;
using savnmore.Models;
using savnmore.Services;

namespace savnmore.Controllers
{
    [Authorize(Roles = Constants.AdministratorsRole)]
    public class ManageSiteController : Controller
    {
        private readonly savnmoreEntities _db = new savnmoreEntities();
        //
        // GET: /ManageSite/

        public ActionResult Index()
        {
            //page list the chains
            var chains = _db.Chains.ToList();
            return View(chains);
        }

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Chain/Create

        [HttpPost]
        public ActionResult Create(Chain chain)
        {
            if (ModelState.IsValid)
            {
                _db.Chains.Add(chain);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(chain);
        }
        //
        // GET: /Store/Create

        public ActionResult CreateNewStore(int id)
        {
            ViewBag.ChainId = id;
            return View();
        }

        //
        // POST: /Store/Create

        [HttpPost]
        public ActionResult CreateNewStore(Store store, int chainId)
        {
            ViewBag.ChainId = chainId;
            if (ModelState.IsValid)
            {
                Chain c = _db.Chains.Find(chainId);
                if (c.Stores == null)
                {
                    c.Stores = new List<Store>();
                }
                c.Stores.Add(store);

                _db.SaveChanges();
                return RedirectToAction("Details", new { id = chainId });
            }
            return View(store);
        }
        //
        // GET: /Chain/Edit/5

        public ActionResult Edit(int id)
        {
            Chain chain = _db.Chains.Find(id);

            return View(chain);
        }

        //
        // POST: /Chain/Edit/5

        [HttpPost]
        public ActionResult Edit(Chain chain)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(chain).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(chain);
        }
        //
        // GET: /Store/Edit/5

        public ActionResult EditStore(int id)
        {
            Store store = _db.Stores.Find(id);
            return View(store);
        }

        //
        // POST: /Store/Edit/5

        [HttpPost]
        public ActionResult EditStore(Store store)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(store).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
                _db.Entry(store.Address).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
                Chain chain = _db.Chains.Include("Stores").Single(i => i.Id == store.Id);
                return RedirectToAction("Details", new { id = chain.Id });
            }
            return View(store);
        }
        //
        // GET: /Chain/Delete/5

        public ActionResult Delete(int id)
        {
            Chain chain = _db.Chains.Find(id);
            return View(chain);
        }

        //
        // POST: /Chain/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Chain chain = _db.Chains.Find(id);
            _db.Chains.Remove(chain);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteWeeklySale(int id)
        {
            WeeklySale weeklysale = _db.WeeklySales.Find(id);
            ViewBag.StoreId = GetStore(weeklysale).Id;
            return View(weeklysale);
        }

        //
        // POST: /WeeklySale/Delete/5

         [HttpPost]
        public ActionResult DeleteSale(int id)
        {
            WeeklySale weeklysale = _db.WeeklySales.Find(id);
            //get this store
             Store s = GetStore(weeklysale);
             Chain chain = _db.Chains.Single(i => i.Stores.Any(p => p.Id == s.Id));
            _db.Entry(weeklysale).State = (System.Data.Entity.EntityState)EntityState.Deleted;
            _db.SaveChanges();
            return RedirectToAction("Details",new{id=chain.Id});
        }

        public ViewResult Details(int id)
        {

            Chain chain = _db.Chains.Include("Stores").Single(i => i.Id == id);
            return View(chain);
        }
        public ViewResult CreateWeeklySale(int id)
        {

            ViewBag.StoreId = id;
            var ws = new WeeklySale {StartsOn = DateTime.Now, EndsOn = DateTime.Now.AddDays(6)};
            return View(ws);
        }
        [HttpPost]
        public ActionResult CreateWeeklySale(WeeklySale weeklysale, int storeId)
        {
            ViewBag.StoreId = storeId;
            if (ModelState.IsValid)
            {
                //get the store
                Store s = _db.Stores.Single(i => i.Id == storeId);
                //if weeklysales are null add new
                if (s.WeeklySales == null) { s.WeeklySales = new List<WeeklySale>(); }
                //add new weeklysale
                s.WeeklySales.Add(weeklysale);
                //savechanges
                _db.SaveChanges();
                Chain chain = _db.Chains.Include("Stores").Single(i => i.Id == storeId);
                return RedirectToAction("Details",new { id = chain.Id });
            }

            return View(weeklysale);
        }
        public ActionResult GetSale(int storeId)
        {
            //get the store
            var sb = new StringBuilder();
            try
            {
                Store s = _db.Stores.Single(i => i.Id == storeId);
                //get this chain for redirect
                var chainid = _db.Chains.Single(i => i.Stores.Any(p => p.Id == s.Id)).Id;
                sb.AppendLine("Getting sale for " + s.Name);
                sb.AppendLine("<br/>");
                var factory = new WeeklySaleServiceFactory();
                IWeeklySaleService svc = factory.GetService(s);
                var ws = svc.GetWeeklySale(s);
                sb.AppendLine(ws.StartsOn.ToShortDateString());
                sb.AppendLine("<br/>");
                sb.AppendLine(ws.EndsOn.ToShortDateString());
                sb.AppendLine("<br/>");
                if (s.WeeklySales.Any(p => p.EndsOn == ws.EndsOn))
                {
                    //we have this sale
                    sb.AppendLine("We have this sale already");

                    return Content(sb.ToString());
                }
                 
                    //this sale is new add it
                    _db.Stores.Attach(s);
                    if (s.WeeklySales == null)
                    {
                        s.WeeklySales = new List<WeeklySale>();
                    }
                    //add new weeklysale
                    s.WeeklySales.Add(ws);
                    _db.SaveChanges();
                    return RedirectToAction("Details", new { id = chainid });
                
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        var line = string.Format("Property: {0} Error: {1}", validationError.PropertyName,
                        validationError.ErrorMessage);
                        Trace.TraceInformation(line);
                        sb.AppendLine(line);
                        sb.AppendLine("<br/>");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
                
               if( ex.InnerException != null)
               {
                   sb.AppendLine("<br/>");
                   sb.AppendLine(ex.InnerException.Message);
               }
            }
            return Content(sb.ToString());
        }

        public ActionResult DeleteAllSales(int id)
        {
             try
            {
            //get all stores for this chain
            Chain chain = _db.Chains.Include("Stores").Single(i => i.Id == id);
            List<WeeklySale> sales = new List<WeeklySale>();
            foreach (var store in chain.Stores)
            {
                sales.AddRange( store.WeeklySales);               
            }
            
            foreach (var sale in sales)
            {
                WeeklySale weeklysale = _db.WeeklySales.Find(sale.Id);
                //get this store
                 
                _db.Entry(weeklysale).State = (System.Data.Entity.EntityState)EntityState.Deleted;
                
            }
            _db.SaveChanges();
            return RedirectToAction("Details", new { id = chain.Id });
            }
             catch (Exception ex)
             {
                 return Content(ex.Message);
             }
        }
        public ActionResult GetAllSales(int id)
        {
            //get each store
            //ccal get sale

            try
            {
                Chain chain = _db.Chains.Include("Stores").Single(i => i.Id == id);
                var factory = new WeeklySaleServiceFactory();
                foreach (var s in chain.Stores)
                {
                    IWeeklySaleService svc = factory.GetService(s);
                    var ws = svc.GetWeeklySale(s);
                    if (s.WeeklySales.Any(p => p.EndsOn == ws.EndsOn))
                    {
                       continue;  //we have this sale 
                    }
                    _db.Stores.Attach(s); //this sale is new add it
                    if (s.WeeklySales == null)
                    {
                        s.WeeklySales = new List<WeeklySale>();
                    }
                    s.WeeklySales.Add(ws);   //add new weeklysale            
                }
                _db.SaveChanges();
                return RedirectToAction("Details", new { id = chain.Id });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        public Store GetStore(WeeklySale ws)
        {
            return _db.Stores.Single(i => i.WeeklySales.Any(p => p.Id == ws.Id));
        }
    }
}
