using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace savnmore.Models
{
    public class ZipCodeEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        [Display(Name = "Zip")]
        [Required(ErrorMessage = "Zip is required.")]
        public string ZipCode { get; set; }
        [Display(Name = "Latitude")]
        [Required(ErrorMessage = "Latitude is required.")]
        public double Latitude { get; set; }
        [Display(Name = "Longitude")]
        [Required(ErrorMessage = "Longitude is required.")]
        public double Longitude { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        public int? StateId { get; set; }
    }
    public class MaxDistance
    {
        public MaxDistance()
        {
            ZipCodes = new List<ZipCodeEntry>();
            CloseStores = new List<Store>();
        }
        public string ZipCode { get; set; }
        public double Miles { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double MaxLatitude { get; set; }
        public double MinLatitude { get; set; }
        public double MinLongitude { get; set; }
        public double MaxLongitude { get; set; }
        public List<ZipCodeEntry> ZipCodes { get; set; }
        public List<Store> CloseStores { get; set; }

    }
    
}