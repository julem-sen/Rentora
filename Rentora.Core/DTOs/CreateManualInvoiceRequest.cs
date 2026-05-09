namespace Rentora.Core.DTOs
{
    public class CreateManualInvoiceRequest
    {
        public string TenantId { get; set; } // The hidden GUID of the tenant
        public DateTime DueDate { get; set; }

        // A list of whatever custom charges the Admin wants to add
        public List<ManualInvoiceItemDto> Items { get; set; } = new List<ManualInvoiceItemDto>();
    }
}
