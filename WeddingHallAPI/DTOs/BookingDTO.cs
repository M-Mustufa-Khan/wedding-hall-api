namespace WeddingHallAPI.DTOs
{
    public class CreateBookingDTO
    {
        public int UserID { get; set; }
        public int HallID { get; set; }
        public int? PackageID { get; set; }
        public DateTime EventDate { get; set; }
        public string? EventType { get; set; }
        public int? GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialNotes { get; set; }

        public string? CNIC { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionRef { get; set; }
        public string? ReceiptImage { get; set; }
    }

    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ContactDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}