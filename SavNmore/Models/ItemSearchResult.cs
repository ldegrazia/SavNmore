using System.Collections.Generic;

namespace savnmore.Models
{
    public class ItemSearchResult
    {
        public Store Store { get; set; }
        public List<Item> RelatedItems { get; set; }
    }
}