namespace Booking_Rapid.Dtos
{
    public class HotelSearchDto
    {
        public string Destination { get; set; } = "Paris";
        public string ArrivalDate { get; set; } = "2026-05-01";
        public string DepartureDate { get; set; } = "2026-05-05";
        public int Adults { get; set; } = 2;
        public List<HotelDto> Hotels { get; set; } = new List<HotelDto>();
        public string Message { get; set; } = "";
    }

    public class HotelDto
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = "";
        public string ReviewScore { get; set; } = "";
        public string ReviewScoreWord { get; set; } = "";
        public int ReviewCount { get; set; }
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
        public List<string> Photos { get; set; } = new List<string>();
        public List<string> Facilities { get; set; } = new List<string>();
        public string Price { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Checkin { get; set; } = "";
        public string Checkout { get; set; } = "";
        public string PropertyClass { get; set; } = "";
        public string AccommodationType { get; set; } = "";
        public int AvailableRooms { get; set; }
        public string Destination { get; set; } = "";
        public string ArrivalDate { get; set; } = "";
        public string DepartureDate { get; set; } = "";
        public int Adults { get; set; }
    }
}
