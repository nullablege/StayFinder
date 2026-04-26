using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Booking_Rapid.Dtos;
using Booking_Rapid.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booking_Rapid.Controllers
{
    public class HomeController : Controller
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string rapidApiKey = "RapidApiKey";



        public async Task<IActionResult> Index()
        {
            var model = new DashboardDto();

            model.Weather = await GetWeather();
            model.ExchangeRates = await GetExchangeRates();
            model.Fuel = await GetFuel();
            model.CryptoRates = await GetCryptoRates();
            model.News = await GetNews();
            model.Food = await GetFood();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Hotels(string destination = "Paris", string arrivalDate = "2026-05-01", string departureDate = "2026-05-05", int adults = 2)
        {
            var model = new HotelSearchDto
            {
                Destination = string.IsNullOrWhiteSpace(destination) ? "Paris" : destination,
                ArrivalDate = string.IsNullOrWhiteSpace(arrivalDate) ? "2026-05-01" : arrivalDate,
                DepartureDate = string.IsNullOrWhiteSpace(departureDate) ? "2026-05-05" : departureDate,
                Adults = adults < 1 ? 1 : adults
            };

            model.Hotels = await SearchHotels(model.Destination, model.ArrivalDate, model.DepartureDate, model.Adults);

            if (model.Hotels.Count == 0)
            {
                model.Message = "No hotels found";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int hotelId, string destination = "Paris", string arrivalDate = "2026-05-01", string departureDate = "2026-05-05", int adults = 2)
        {
            var hotel = await GetHotelDetail(hotelId, destination, arrivalDate, departureDate, adults);

            if (hotel == null)
            {
                return RedirectToAction("Hotels");
            }

            return View(hotel);
        }

        public IActionResult Privacy()
        {
            return RedirectToAction("Index");
        }

        private async Task<WeatherDto> GetWeather()
        {
            try
            {
                using var document = await GetJson("https://open-weather13.p.rapidapi.com/city?city=paris&lang=EN", "open-weather13.p.rapidapi.com");
                var root = document.RootElement;
                var weather = new WeatherDto
                {
                    City = GetString(root, "name"),
                    Country = GetString(GetObject(root, "sys"), "country"),
                    Temperature = GetDecimalString(GetObject(root, "main"), "temp"),
                    Condition = ""
                };

                if (root.TryGetProperty("weather", out var weatherArray) && weatherArray.ValueKind == JsonValueKind.Array && weatherArray.GetArrayLength() > 0)
                {
                    var first = weatherArray[0];
                    weather.Condition = GetString(first, "description");
                    weather.Icon = GetString(first, "icon");
                }

                return weather;
            }
            catch
            {
                return new WeatherDto();
            }
        }

        private async Task<List<ExchangeDto>> GetExchangeRates()
        {
            var rates = new List<ExchangeDto>();
            var pairs = new List<(string From, string To)> { ("USD", "EUR"), ("GBP", "EUR"), ("JPY", "EUR") };

            foreach (var pair in pairs)
            {
                try
                {
                    var url = $"https://currency-conversion-and-exchange-rates.p.rapidapi.com/convert?from={pair.From}&to={pair.To}&amount=1";
                    using var document = await GetJson(url, "currency-conversion-and-exchange-rates.p.rapidapi.com");
                    rates.Add(new ExchangeDto
                    {
                        Pair = $"{pair.From}/{pair.To}",
                        Rate = GetDecimalString(GetObject(document.RootElement, "info"), "rate")
                    });
                }
                catch
                {
                }
            }

            return rates;
        }

        private async Task<FuelDto> GetFuel()
        {
            try
            {
                using var document = await GetJson("https://gas-price.p.rapidapi.com/europeanCountries", "gas-price.p.rapidapi.com");

                if (document.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in result.EnumerateArray())
                    {
                        if (GetString(item, "country") == "France")
                        {
                            return new FuelDto
                            {
                                Country = GetString(item, "country"),
                                Currency = GetString(item, "currency"),
                                Gasoline = GetString(item, "gasoline"),
                                Diesel = GetString(item, "diesel"),
                                Lpg = GetString(item, "lpg")
                            };
                        }
                    }
                }

                return new FuelDto();
            }
            catch
            {
                return new FuelDto();
            }
        }

        private async Task<List<CryptoDto>> GetCryptoRates()
        {
            try
            {
                using var document = await GetJson("https://fast-price-exchange-rates.p.rapidapi.com/api/v1/convert-rates/crypto/from?detailed=false&currency=BTC", "fast-price-exchange-rates.p.rapidapi.com");
                var list = new List<CryptoDto>();

                if (document.RootElement.TryGetProperty("to", out var to))
                {
                    AddCrypto(list, to, "Ethereum", "ETH");
                    AddCrypto(list, to, "Solana", "SOL");
                    AddCrypto(list, to, "Tether", "USDT");
                    AddCrypto(list, to, "Cardano", "ADA");
                }

                return list;
            }
            catch
            {
                return new List<CryptoDto>();
            }
        }

        private async Task<List<NewsDto>> GetNews()
        {
            try
            {
                using var document = await GetJson("https://real-time-news-data.p.rapidapi.com/search?query=Travel&limit=4&time_published=anytime&country=US&lang=en", "real-time-news-data.p.rapidapi.com");
                var news = new List<NewsDto>();

                if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in data.EnumerateArray().Take(4))
                    {
                        news.Add(new NewsDto
                        {
                            Title = GetString(item, "title"),
                            Snippet = GetString(item, "snippet"),
                            Source = GetString(item, "source_name"),
                            PhotoUrl = GetString(item, "photo_url")
                        });
                    }
                }

                return news;
            }
            catch
            {
                return new List<NewsDto>();
            }
        }

        private async Task<FoodDto> GetFood()
        {
            try
            {
                using var document = await GetJson("https://tasty.p.rapidapi.com/recipes/list?from=0&size=20&tags=under_30_minutes", "tasty.p.rapidapi.com");

                if (document.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        var name = GetString(item, "name");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var minutes = GetInt(item, "cook_time_minutes");

                            return new FoodDto
                            {
                                Name = name,
                                Description = GetString(item, "description"),
                                ImageUrl = GetString(item, "thumbnail_url"),
                                CookTime = minutes > 0 ? $"{minutes} minutes" : ""
                            };
                        }
                    }
                }

                return new FoodDto();
            }
            catch
            {
                return new FoodDto();
            }
        }

        private async Task<List<HotelDto>> SearchHotels(string destination, string arrivalDate, string departureDate, int adults)
        {
            try
            {
                var destinationInfo = await GetDestination(destination);
                if (string.IsNullOrWhiteSpace(destinationInfo.DestId))
                {
                    return new List<HotelDto>();
                }

                var url = "https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels" +
                          $"?dest_id={Uri.EscapeDataString(destinationInfo.DestId)}" +
                          $"&search_type={Uri.EscapeDataString(destinationInfo.SearchType)}" +
                          $"&arrival_date={Uri.EscapeDataString(arrivalDate)}" +
                          $"&departure_date={Uri.EscapeDataString(departureDate)}" +
                          $"&adults={adults}" +
                          "&room_qty=1&page_number=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR&location=US";

                using var document = await GetJson(url, "booking-com15.p.rapidapi.com");
                var hotels = new List<HotelDto>();

                if (document.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("hotels", out var hotelArray) &&
                    hotelArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in hotelArray.EnumerateArray().Take(20))
                    {
                        var property = GetObject(item, "property");
                        var photos = GetPhotos(property);
                        var hotel = new HotelDto
                        {
                            HotelId = GetInt(property, "id"),
                            Name = GetString(property, "name"),
                            ReviewScore = GetDecimalString(property, "reviewScore"),
                            ReviewScoreWord = GetString(property, "reviewScoreWord"),
                            ReviewCount = GetInt(property, "reviewCount"),
                            Description = CleanDescription(GetString(item, "accessibilityLabel")),
                            PhotoUrl = photos.FirstOrDefault() ?? "",
                            Photos = photos,
                            Price = GetDecimalString(GetObject(GetObject(property, "priceBreakdown"), "grossPrice"), "value"),
                            Currency = GetString(property, "currency"),
                            Checkin = FormatTimeRange(GetObject(property, "checkin")),
                            Checkout = FormatTimeRange(GetObject(property, "checkout")),
                            PropertyClass = GetInt(property, "propertyClass").ToString(),
                            Destination = destination,
                            ArrivalDate = arrivalDate,
                            DepartureDate = departureDate,
                            Adults = adults
                        };

                        if (hotel.HotelId > 0 && !string.IsNullOrWhiteSpace(hotel.Name))
                        {
                            hotels.Add(hotel);
                        }
                    }
                }

                return hotels;
            }
            catch
            {
                return new List<HotelDto>();
            }
        }

        private async Task<HotelDto?> GetHotelDetail(int hotelId, string destination, string arrivalDate, string departureDate, int adults)
        {
            try
            {
                var url = "https://booking-com15.p.rapidapi.com/api/v1/hotels/getHotelDetails" +
                          $"?hotel_id={hotelId}" +
                          $"&adults={adults}" +
                          $"&arrival_date={Uri.EscapeDataString(arrivalDate)}" +
                          $"&departure_date={Uri.EscapeDataString(departureDate)}" +
                          "&children_age=1%2C17&room_qty=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR";

                using var document = await GetJson(url, "booking-com15.p.rapidapi.com");

                if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var price = GetObject(GetObject(data, "product_price_breakdown"), "gross_amount");
                if (price.ValueKind == JsonValueKind.Undefined)
                {
                    price = GetObject(GetObject(data, "composite_price_breakdown"), "gross_amount");
                }

                var photos = GetDetailPhotos(data);
                var facilities = GetDetailFacilities(data);
                var description = GetRoomDescription(data);
                var city = GetString(data, "city_name_en");
                if (string.IsNullOrWhiteSpace(city))
                {
                    city = GetString(data, "city");
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    description = $"{GetString(data, "hotel_name")} is located in {city}.";
                }

                return new HotelDto
                {
                    HotelId = GetInt(data, "hotel_id"),
                    Name = GetString(data, "hotel_name"),
                    ReviewCount = GetInt(data, "review_nr"),
                    Description = description,
                    Address = GetString(data, "address"),
                    City = city,
                    Country = GetString(data, "country_trans"),
                    PhotoUrl = photos.FirstOrDefault() ?? "",
                    Photos = photos,
                    Facilities = facilities,
                    Price = GetDecimalString(price, "value"),
                    Currency = GetString(price, "currency"),
                    AccommodationType = GetString(data, "accommodation_type_name"),
                    AvailableRooms = GetInt(data, "available_rooms"),
                    Destination = destination,
                    ArrivalDate = arrivalDate,
                    DepartureDate = departureDate,
                    Adults = adults
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<(string DestId, string SearchType)> GetDestination(string query)
        {
            try
            {
                var url = $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchDestination?query={Uri.EscapeDataString(query)}";
                using var document = await GetJson(url, "booking-com15.p.rapidapi.com");

                if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in data.EnumerateArray())
                    {
                        var destId = GetString(item, "dest_id");
                        if (string.IsNullOrWhiteSpace(destId))
                        {
                            destId = GetString(item, "destId");
                        }

                        if (!string.IsNullOrWhiteSpace(destId))
                        {
                            var searchType = GetString(item, "search_type");
                            if (string.IsNullOrWhiteSpace(searchType))
                            {
                                searchType = "CITY";
                            }

                            return (destId, searchType.ToUpperInvariant());
                        }
                    }
                }

                return ("", "CITY");
            }
            catch
            {
                return ("", "CITY");
            }
        }

        private async Task<JsonDocument> GetJson(string url, string host)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            request.Headers.Add("x-rapidapi-key", rapidApiKey);
            request.Headers.Add("x-rapidapi-host", host);

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(body);
        }

        private void AddCrypto(List<CryptoDto> list, JsonElement to, string name, string symbol)
        {
            var value = GetDecimalString(to, symbol);
            if (!string.IsNullOrWhiteSpace(value))
            {
                list.Add(new CryptoDto
                {
                    Name = name,
                    Symbol = symbol,
                    Value = value
                });
            }
        }

        private List<string> GetPhotos(JsonElement property)
        {
            var photos = new List<string>();

            if (property.TryGetProperty("photoUrls", out var photoUrls) && photoUrls.ValueKind == JsonValueKind.Array)
            {
                foreach (var photo in photoUrls.EnumerateArray().Take(3))
                {
                    var value = photo.ValueKind == JsonValueKind.String ? photo.GetString() : "";
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        photos.Add(value);
                    }
                }
            }

            return photos;
        }

        private List<string> GetDetailPhotos(JsonElement data)
        {
            var photos = new List<string>();

            if (data.TryGetProperty("rooms", out var rooms) && rooms.ValueKind == JsonValueKind.Object)
            {
                foreach (var room in rooms.EnumerateObject())
                {
                    if (room.Value.TryGetProperty("photos", out var roomPhotos) && roomPhotos.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var photo in roomPhotos.EnumerateArray())
                        {
                            var url = GetFirstString(photo, "url_max750", "url_max1280", "url_original", "url_max300");
                            if (!string.IsNullOrWhiteSpace(url) && !photos.Contains(url))
                            {
                                photos.Add(url);
                            }

                            if (photos.Count == 3)
                            {
                                return photos;
                            }
                        }
                    }
                }
            }

            return photos;
        }

        private List<string> GetDetailFacilities(JsonElement data)
        {
            var facilities = new List<string>();
            var block = GetObject(data, "facilities_block");

            if (block.ValueKind == JsonValueKind.Object && block.TryGetProperty("facilities", out var facilityArray) && facilityArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in facilityArray.EnumerateArray().Take(12))
                {
                    var name = GetString(item, "name");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        facilities.Add(name);
                    }
                }
            }

            if (facilities.Count == 0 && data.TryGetProperty("property_highlight_strip", out var highlights) && highlights.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in highlights.EnumerateArray().Take(12))
                {
                    var name = GetString(item, "name");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        facilities.Add(name);
                    }
                }
            }

            if (facilities.Count == 0 && data.TryGetProperty("family_facilities", out var familyFacilities) && familyFacilities.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in familyFacilities.EnumerateArray().Take(12))
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        facilities.Add(item.GetString() ?? "");
                    }
                }
            }

            return facilities;
        }

        private string GetRoomDescription(JsonElement data)
        {
            if (data.TryGetProperty("rooms", out var rooms) && rooms.ValueKind == JsonValueKind.Object)
            {
                foreach (var room in rooms.EnumerateObject())
                {
                    var description = GetString(room.Value, "description");
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        return description;
                    }
                }
            }

            return "";
        }

        private string FormatTimeRange(JsonElement item)
        {
            var from = GetString(item, "fromTime");
            var until = GetString(item, "untilTime");

            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(until))
            {
                return $"{from} - {until}";
            }

            return from + until;
        }

        private string CleanDescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            var lines = value.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Replace(((char)8206).ToString(), "").Replace(((char)8236).ToString(), "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(5);

            return string.Join(" ", lines);
        }

        private JsonElement GetObject(JsonElement item, string propertyName)
        {
            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty(propertyName, out var value))
            {
                return value;
            }

            return default;
        }

        private string GetString(JsonElement item, string propertyName)
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty(propertyName, out var value))
            {
                return "";
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? "";
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.ToString();
            }

            return "";
        }

        private int GetInt(JsonElement item, string propertyName)
        {
            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty(propertyName, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                {
                    return number;
                }

                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number))
                {
                    return number;
                }
            }

            return 0;
        }

        private string GetDecimalString(JsonElement item, string propertyName)
        {
            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty(propertyName, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
                {
                    return number.ToString("0.##", CultureInfo.InvariantCulture);
                }

                if (value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString() ?? "";
                }
            }

            return "";
        }

        private string GetFirstString(JsonElement item, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var value = GetString(item, propertyName);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return "";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
