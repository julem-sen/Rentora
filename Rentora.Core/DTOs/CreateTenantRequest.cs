namespace Rentora.Core.DTOs
{
    public class CreateTenantRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UnitNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public decimal MonthlyRentAmount { get; set; }
        public int MonthsAdvance { get; set; } // e.g., 0, 1, or 2
        public int MonthsDeposit { get; set; } // e.g., 0, 1, or 2
        public DateTime LeaseStartDate { get; set; }

    }
}
