using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Linq;
using System.Text;
namespace savnmore.Models
{
    public class ShoppingList
    {
        public List<StoreShoppingList> ListOfStores { get; set; }
        public  string PrintList()
        {
            StringBuilder sb = new StringBuilder();
            //header
            sb.AppendLine("My savnmore.com Shopping List<br/>");
            if (this.ListOfStores == null)
            {
                return "No items on list";
            }
            foreach (StoreShoppingList sl in ListOfStores)
            {
                //print
                sb.AppendLine(sl.PrintList());
                //spacer
            }
            return (sb.ToString());
            //footer
        }
         
    }
    public class StoreShoppingList
    {
        public Store Store { get; set; }
        public List<Item> Items { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public double Total
        {
            get
            {
                double d = 0;
                foreach (var s in Items)
                {
                    if (s.SalePrice.HasValue)
                    {
                        d += s.Quantity * s.SalePrice.Value;
                    }
                }
                return d;

            }
        }
        //add to list
        //remove from list
        //add store
        //remove store
        public string PrintList()
        {
            //print
            StringBuilder sb = new StringBuilder();
            if (this.Store == null)
            {
                return "No stores for list";
            }
            sb.AppendLine("<h3>" +  Store.Name + "</h3>");
            sb.AppendLine( Store.Address.Address1 + "<br/>" +  Store.Address.City +  Store.Address.State + "," +  Store.Address.Zip);
            sb.AppendLine("<br/><table><tr><th>Qty</th><th>Item</th><th>Description</th><th>Price<th></tr>");
            foreach (Item i in  Items)
            {
                sb.AppendLine("<tr><td>" + i.Quantity + "</td><td>" + i.Name + "</td><td><div style='max-width:400px;'>" + i.Description + "</div></td><td>" + i.Price + "<td></tr>");
            }
            sb.AppendLine("<tr><td> </td><td>Approximate Store Total</td><td>" +  Total.ToString("c0") + "<td></tr>");
            sb.AppendLine("<h3>&nbsp;</h3>");
            return sb.ToString();
        }
        
    }
    public class ShoppingListService
    {
        private savnmoreEntities db = new savnmoreEntities();
        public const string List = "list";
        //gets a list from the session
        public ShoppingListService()
        {
            if(HttpContext.Current.Session[List] == null)
            {
                ShoppingList sl = new ShoppingList();
                sl.ListOfStores = new List<StoreShoppingList>();
                HttpContext.Current.Session[List] = sl;
            }
        }
        public ShoppingList GetList()
        {
            return HttpContext.Current.Session[List] as ShoppingList;
        }
        public void UpdateList(ShoppingList list)
        {
            //find everystoreid without items
            List<int> storeIds = (from t in list.ListOfStores where !t.Items.Any() select t.Store.Id).ToList();           
            foreach (var i in storeIds)
            {
                var removeStore = list.ListOfStores.Single(t => t.Store.Id == i);
                list.ListOfStores.Remove(removeStore);
            }
            SaveList(list);
        }
        /// <summary>
        /// Updates the shopping list session
        /// </summary>
        /// <param name="updatedList"></param>
        public void SaveList(ShoppingList updatedList)
        {
            HttpContext.Current.Session[List] = updatedList;
        }
        public bool StoreIsOnList(string storeName)
        {
            var stores = GetList().ListOfStores;
            return stores.Any(t => t.Store.Name == storeName);
        }
        public bool StoreIsOnList(int storeId)
        {
            var stores = GetList().ListOfStores;
            return stores.Any(t => t.Store.Id == storeId);
        }
        public Store GetStore(int storeId)
        {
            return GetList().ListOfStores.Single(t => t.Store.Id == storeId).Store;
        }
        public void AddStore(int storeId)
        {
            if(!StoreIsOnList(storeId))
            {
                var sl = GetList();
                StoreShoppingList stores = new StoreShoppingList();
                //hit the database and get the store
                stores.Store = db.Stores.Single(t=>t.Id == storeId);
                stores.Items = new List<Item>();
                sl.ListOfStores.Add(stores);
                HttpContext.Current.Session[List] = sl;
            }
        }
        public StoreShoppingList GetShoppingListForStore(int storeId)
        {
            return GetList().ListOfStores.Single(t => t.Store.Id == storeId);
        }
        public List<Item> GetShoppingListItemsForStore(int storeId)
        {
            return GetShoppingListForStore(storeId).Items;
        }
        public void RemoveStore(int storeId)
        {
            if (StoreIsOnList(storeId))
            {
                var sl = GetList();
                var removeStore =sl.ListOfStores.Single(t => t.Store.Id == storeId);
                sl.ListOfStores.Remove(removeStore);
                HttpContext.Current.Session[List] = sl;
            }
        }
        public void UpdateStoreShoppingList(StoreShoppingList newList)
        {
            //get the current list
            // update the StoreShoppingList for this store
            var current = GetList();
            //find the store for this newlist 
            try
            {
                var replaceStore = current.ListOfStores.Single(t => t.Store.Id == newList.Store.Id);
                current.ListOfStores.Remove(replaceStore);
            }
            catch (Exception) //store is null
            {
            }
            current.ListOfStores.Add(newList);
            UpdateList(current);
        }
        public bool IsItemOnList(StoreShoppingList currentList, int itemId)
        {
            return currentList.Items.Any(t => t.Id == itemId);
        }
        /// <summary>
        /// Returns true if the item for this store is on the users shopping list
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool IsItemOnList(int storeId, int itemId)
        {
            if (!StoreIsOnList(storeId))
            {
                return false;
            }
            var usersList = GetShoppingListForStore(storeId);
            return IsItemOnList(usersList, itemId);
        }
        public void AddItem(int storeId, int itemId)
        {
            //get this store, if there is not a store in the list then add it
            AddStore(storeId);
            StoreShoppingList sl = GetShoppingListForStore(storeId);
            Item itm;
            if(!IsItemOnList(sl,itemId))
            {
                //get the item
                itm = db.SaleItems.Single(t => t.Id == itemId);
                itm.Quantity++;
                itm.OnList = true;
                sl.Items.Add(itm);
            }
            else
            {
                //get the item from the list
                itm = sl.Items.Single(t => t.Id == itemId);
                itm.Quantity++; //update the quantity
                itm.OnList = true;
            }
            UpdateStoreShoppingList(sl);
        }
        public string AddItemUpdateTotal(int storeId, int itemId)
        {
            //get this store, if there is not a store in the list then add it
            AddStore(storeId);
            StoreShoppingList sl = GetShoppingListForStore(storeId);
            Item itm;
            if (!IsItemOnList(sl, itemId))
            {
                //get the item
                itm = db.SaleItems.Single(t => t.Id == itemId);
                itm.Quantity++;
                itm.OnList = true;
                sl.Items.Add(itm);
            }
            else
            {
                //get the item from the list
                itm = sl.Items.Single(t => t.Id == itemId);
                itm.Quantity++; //update the quantity
                itm.OnList = true;
            }
            UpdateStoreShoppingList(sl);
            return sl.Total.ToString("c");
        }

        public string RemoveItem(int storeId, int itemId)
        {
            //get this store, if there is not a store then leave
            double total = 0;
            if(StoreIsOnList(storeId))
            {
               //get the store
                //if the item is in the list remove it
                var sl = GetShoppingListForStore(storeId);
                if(IsItemOnList(sl,itemId))
                {
                    //removeit
                   var itm = sl.Items.Single(t => t.Id == itemId);
                    sl.Items.Remove(itm);
                    UpdateStoreShoppingList(sl);
                    
                    return sl.Total.ToString("c");
                }
            }
            return total.ToString("c");
        }
        public string Remove1ItemUpdateTotal(int storeId, int itemId)
        {
            //get this store, if there is not a store then leave
            double total = 0;
            if (StoreIsOnList(storeId))
            {
                //get the store
                //if the item is in the list remove one
                var sl = GetShoppingListForStore(storeId);
                if (IsItemOnList(sl, itemId))
                {
                    //removeit
                    var itm = sl.Items.Single(t => t.Id == itemId);
                    itm.Quantity--;
                    UpdateStoreShoppingList(sl);
                    return sl.Total.ToString("c");
                }
                 
            }
            return total.ToString("c");
        }
        public string GetTotal(int storeId)
        {
             double total = 0;
             if (StoreIsOnList(storeId))
             {
                 //get the store
                 //if the item is in the list remove it
                 var sl = GetShoppingListForStore(storeId);
                 return sl.Total.ToString("c");
             }
            return total.ToString("c");
        }
        public static bool ItemIsOnList(List<Item> items,Item targetItem)
        {
            return items.Any(t => t.Id == targetItem.Id);
        }
        /// <summary>
        /// Loops over the items, and checks to see if any of the items are on the users shopping list
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="currentItems"></param>
        /// <returns></returns>
        public List<Item> MarkWhatIsOnUsersShoppingList(int storeId,List<Item> currentItems)
        {
            if(!StoreIsOnList(storeId))
            {
                return currentItems;
            }
            var usersList = GetShoppingListForStore(storeId);
            foreach (var currentItem in currentItems)
            {
                if(ItemIsOnList(usersList.Items,currentItem))
                {
                    currentItem.OnList = true;
                    //need quantity
                    //get the item from the list
                    currentItem.Quantity = usersList.Items.Single(t => t.Id == currentItem.Id).Quantity;
                }
            }
            return currentItems;
        }
    }   
}