using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using savnmore.Models;

namespace savnmore.Services
{
    public static class SortingService
    {
        public const string AtoZ = "AtoZ";
        public const string ZtoA = "ZtoA";
        public const string LowPrice = "LowPrice";
        public const string HighPrice = "HighPrice";
        /// <summary>
        /// Sorts the items using the sort order
        /// </summary>
        /// <param name="itms"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static List<Item> SortItems(IEnumerable<Item> itms, string sortOrder)
        {
            switch (sortOrder)
            {
                case ZtoA:
                    {
                        return itms.OrderByDescending(t => t.Name).ToList();
                    }
                case HighPrice:
                    {
                        return itms.OrderByDescending(t => t.SalePrice).ToList();
                    }
                case LowPrice:
                    {
                        return itms.OrderBy(t => t.SalePrice).ToList();
                    }
                default:
                    {
                        return itms.OrderBy(t => t.Name).ToList();
                    }
            }
        }
        /// <summary>
        /// Returns a to z low to high selectlistitems
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetSortOptions(string sortOrder)
        {
            
            List<SelectListItem> miles = new List<SelectListItem>
                                             {
                                                 new SelectListItem
                                                     {
                                                         Value = AtoZ,
                                                         Text = "Name: A to Z"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = ZtoA,
                                                         Text = "Name: Z to A"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = LowPrice ,
                                                         Text = "Price:Low to High"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = HighPrice,
                                                         Text = "Price:High to Low"
                                                     }
                                             };
            if(string.IsNullOrEmpty(sortOrder))
            {
                miles[0].Selected = true;
            }
            else
            {
                 var selected = miles.Single(t => t.Value == sortOrder);
                selected.Selected = true;
            }
           
            return miles;
        }
    }
}