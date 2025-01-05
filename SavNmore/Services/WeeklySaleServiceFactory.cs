using savnmore.Models;
namespace savnmore.Services
{
    public class WeeklySaleServiceFactory
    {
        public IWeeklySaleService GetService(Store store )
        {
            if(store.Name.Contains("FoodTown"))
            {
                return new FoodTownSalesService();
            }
            if(store.Name.Contains("ShopRite"))
            {
                return new ShopRiteSalesService();
            }
            return new FoodTownSalesService();
        }
    }
}