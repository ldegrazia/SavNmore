using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using savnmore.Models;

namespace savnmore.Controllers
{ 
    public class StoreController : Controller
    {
        private readonly savnmoreEntities _db = new savnmoreEntities();

        //
        // GET: /Store/

        
        //
        // GET: /Store/Details/5

        public ViewResult Details(int id)
        {
            Store store = _db.Stores.Single(i => i.Id == id);
            ViewBag.ChainId = _db.Chains.Single(i => i.Stores.Any(p => p.Id == id)).Id;
            return View(store);
        }

         
         
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}