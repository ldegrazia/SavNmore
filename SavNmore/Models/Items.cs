using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using savnmore;

namespace savnmore.Models
{
    public class Chain
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Name")]
        [MaxLength(150)]
        [Required(ErrorMessage = "Name is required.")] 
        public string Name { get; set; }
        [Display(Name = "Photo")]
        public string ImageUrl { get; set; }
        [Display(Name = "Url")]
        [DataType(DataType.Url)]
        [RegularExpression(Constants.UrlRegex, ErrorMessage = ErrorConstants.EntryInvalid)]
        public string Url { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
    }
    public class Store
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Name")]
        [MaxLength(150)]
        [Required(ErrorMessage = "Name is required.")] 
        public string Name { get; set; }
        public virtual Address Address { get; set; }
        [Display(Name = "Phone")]
        [MaxLength(20)]
        public string Phone { get; set; }
        [Display(Name = "Photo")]
        public string ImageUrl { get; set; }
        [Display(Name = "Url")]
        [DataType(DataType.Url)]
        [RegularExpression(Constants.UrlRegex, ErrorMessage = ErrorConstants.EntryInvalid)]
        public string Url { get; set; }
        public virtual ICollection<WeeklySale> WeeklySales { get; set; }
         
    }
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required.")] 
        [MaxLength(150)] 
        public string Name { get; set; }
    }
    public class Item : IComparable

    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required.")] 
        [MaxLength(255)] 
        public string Name { get; set; }
        [Display(Name = "Description")]
        [MaxLength(500)] 
        public string Description { get; set; }
        [Display(Name = "Price")]
        [MaxLength(255)]
        public string Price { get; set; }
        [Display(Name = "Sale Price")]
        public double? SalePrice { get; set; }
        //public int? CategoryId { get; set; }
        //[ForeignKey("CategoryId")]
        //public virtual Category Category { get; set; }
         [Display(Name = "Photo")]
        public string ImageUrl { get; set; }
         public int WeeklySaleId { get; set; }
         [ForeignKey("WeeklySaleId")]
         public virtual WeeklySale WeeklySale { get; set; }
         [NotMapped]
         [Display(Name = "On List")]
         public bool OnList { get; set; }
         [NotMapped]
         [Display(Name = "Qty")]
         public int Quantity { get; set; }
         [NotMapped]
         [Display(Name = "Notes")]
         [AllowHtml]
         public string Notes { get; set; }


         public int CompareTo(object obj)
         {
             Item i = (Item) obj;
             return System.String.CompareOrdinal(this.Name, i.Name);
         }
    }
    public class WeeklySale
    {
        [Key]
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "A start date is required.")] 
        public DateTime StartsOn { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "An end date is required.")] 
        public DateTime EndsOn { get; set; }
        public virtual ICollection<Item> SaleItems { get; set; }
        public int StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
    }
    public class Address
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name = "Adress1")]
        [MaxLength(150)]
        [Required(ErrorMessage = "Address is required.")]
        public string Address1 { get; set; }
        [Display(Name = "City")]
        [MaxLength(150)]
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }
        [Display(Name = "State")]
        [MaxLength(50)]
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }
        [Display(Name = "Zip")]
        [MaxLength(15)]
        [Required(ErrorMessage = "Zip is required.")]
        public string Zip { get; set; }
    }
    public class ItemComparer : IEqualityComparer<Item>
    {
        #region IEqualityComparer<Item> Members

        public bool Equals(Item x, Item y)
        {
            if(x.Name.Equals(y.Name))
            {
                if(x.Description.Equals(y.Description))
                {
                    return true;
                }
            }
            return false;
        }

        public int GetHashCode(Item obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}