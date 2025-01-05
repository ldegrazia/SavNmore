using System.Linq;
using savnmore.Models;

namespace savnmore.Services
{
    public class CategoryService
    {
        //private readonly savnmoreEntities _db = new savnmoreEntities();
        //public  Category  Get(string name)
        //{
        //    return _db.Categories.First(t => t.Name == name);

        //}
        //public Category Get(int id)
        //{
        //    return _db.Categories.Find(id);

        //}
        public bool Add(string categoryName)
        {
            //if (!_db.Categories.Any(t => t.Name == categoryName))
            //{
            //    //add it
            //    var c = new Category { Name = categoryName };
            //    _db.Categories.Add(c);
            //    _db.SaveChanges();
            //    return true;
            //}
            return false;
        }


    }
}