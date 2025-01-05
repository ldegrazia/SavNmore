using System.Collections.Generic;
using savnmore.Models;

namespace savnmore.Services
{
    public interface IWeeklySaleService
    {
        WeeklySale GetWeeklySale(Store store); 
    }
}
