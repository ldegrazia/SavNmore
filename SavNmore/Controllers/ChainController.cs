using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using savnmore.Models;
using savnmore.Services;
namespace savnmore.Controllers
{ 
    public class ChainController : Controller
    {
        private readonly savnmoreEntities _db = new savnmoreEntities();

        //
        // GET: /Chain/

        public ViewResult Index()
        {
            ViewBag.CurrentZipSearch = LocationService.GetUserZip();
            ViewBag.FindMiles = LocationService.GetMiles();
            var chains = _db.Chains.ToList();
            return View(chains);
        }

        //
        // GET: /Chain/Details/5

        public ActionResult Details(int id)
        {
            DateTime now = DateTime.Now.Date;
            Chain chain = _db.Chains.Include("Stores").Single(i => i.Id == id);
            foreach(Store s in chain.Stores)
            {
                //is this weekly sale expired?
                List<int> weeklysaleids = new List<int>();
                foreach(WeeklySale ws in s.WeeklySales)
                {
                    //if this is expired then get the id
                    if(ws.EndsOn < now)
                    {
                        weeklysaleids.Add(ws.Id);
                    }
                }
                //var expiredSales = s.WeeklySales.Where(p => p.EndsOn < now);
                foreach (var expiredSale in weeklysaleids)//delete these sales
                {
                    DeleteSale(expiredSale);
                }
            }
            try
            {
                    chain = _db.Chains.Include("Stores").Single(i => i.Id == id);
                    var stores = chain.Stores.Where(p => p.WeeklySales.Count == 0);
                    foreach (var store in stores)
                    {
                    
                        GetSale(store.Id);
                    }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            chain = _db.Chains.Include("Stores").Single(i => i.Id == id);
            return View(chain);

            //try to get the weekly sales if there are any stores with expired weekly sales
            
        }
        public void GetSale(int storeId)
        {
            //get the store
            var sb = new StringBuilder();
            try
            {
                Store s = _db.Stores.Single(i => i.Id == storeId);
                //get this chain for redirect
                sb.AppendLine("Getting sale for " + s.Name);
                sb.AppendLine("<br/>");
                var factory = new WeeklySaleServiceFactory();
                IWeeklySaleService svc = factory.GetService(s);
                var ws = svc.GetWeeklySale(s);
                sb.AppendLine(ws.StartsOn.ToShortDateString());
                sb.AppendLine("<br/>");
                sb.AppendLine(ws.EndsOn.ToShortDateString());
                sb.AppendLine("<br/>");
                ws.SaleItems = ws.SaleItems.Distinct().ToList(); //remove dupes
                if (s.WeeklySales.Any(p => p.EndsOn == ws.EndsOn))
                {
                    //we have this sale
                    sb.AppendLine("We have this sale already");
                    System.Diagnostics.Debug.WriteLine(sb.ToString());
                    return;
                }

                //this sale is new add it
                _db.Stores.Attach(s);
                if (s.WeeklySales == null)
                {
                    s.WeeklySales = new List<WeeklySale>();
                }
                //add new weeklysale
                sb.AppendLine("Adding weekly sale for " + ws.EndsOn);
                s.WeeklySales.Add(ws);
                _db.SaveChanges();
                
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

                if (ex.InnerException != null)
                {
                    sb.AppendLine("<br/>");
                    sb.AppendLine(ex.InnerException.Message);
                }
            }
            System.Diagnostics.Debug.WriteLine(sb.ToString());
            
        }
        public void DeleteSale(int id)
        {
              WeeklySale weeklysale = _db.WeeklySales.Find(id);
              if (weeklysale == null)
              {
                  return;
              }
            //get this store
             Store s = GetStore(weeklysale);
             Chain chain = _db.Chains.Single(i => i.Stores.Any(p => p.Id == s.Id));
             _db.Stores.Attach(s);
            _db.Entry(weeklysale).State = EntityState.Deleted;
            _db.SaveChanges();
        }
        public Store GetStore(WeeklySale ws)
        {
            return _db.Stores.Single(i => i.WeeklySales.Any(p => p.Id == ws.Id));
        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}