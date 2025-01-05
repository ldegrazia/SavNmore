using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using savnmore.Models;
using savnmore.Services;

namespace savnmore.Controllers
{
    public class ShoppingListController : Controller
    {
        //
        // GET: /ShoppingList/
        readonly ShoppingListService _svc = new ShoppingListService();
        public ActionResult Index(string sortOrder)
        {
            //get this user's shopping list
            var list = _svc.GetList();
            if (String.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "AtoZ";
            }
            ViewBag.SortOptions = SortingService.GetSortOptions(sortOrder);
            var orderedList = list.ListOfStores.OrderByDescending(t => t.Store.Name);
            // for each store, sort the items
            foreach(var p in orderedList)
            {
                p.Items = SortingService.SortItems(p.Items, sortOrder);
            }
            _svc.SaveList(list);
            return View(orderedList);
        }
        public ViewResult PrintList()
        {
            //get the list and return it
            return View(_svc.GetList().ListOfStores);
        }
        public JsonResult EmailList(string to)
        {
            if(string.IsNullOrEmpty(to))
            {
                return Json( "Please enter a valid address.");
            }
            try
            {

                 EmailService.EmailShoppingList(to);
                 return Json( "Email sent to " + to);
            }
            catch 
            {
                return Json("Could not send email.");
            }
            
        }
    }
}
