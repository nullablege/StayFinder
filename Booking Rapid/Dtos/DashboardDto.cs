namespace Booking_Rapid.Dtos
{
    public class DashboardDto
    {
        public WeatherDto Weather { get; set; } = new WeatherDto();
        public List<ExchangeDto> ExchangeRates { get; set; } = new List<ExchangeDto>();
        public FuelDto Fuel { get; set; } = new FuelDto();
        public List<CryptoDto> CryptoRates { get; set; } = new List<CryptoDto>();
        public List<NewsDto> News { get; set; } = new List<NewsDto>();
        public FoodDto Food { get; set; } = new FoodDto();
    }

    public class WeatherDto
    {
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Temperature { get; set; } = "";
        public string Condition { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public class ExchangeDto
    {
        public string Pair { get; set; } = "";
        public string Rate { get; set; } = "";
    }

    public class FuelDto
    {
        public string Country { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Gasoline { get; set; } = "";
        public string Diesel { get; set; } = "";
        public string Lpg { get; set; } = "";
    }

    public class CryptoDto
    {
        public string Name { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class NewsDto
    {
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
        public string Source { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
    }

    public class FoodDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string CookTime { get; set; } = "";
    }
}
