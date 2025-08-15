namespace HTFP.Shared.Models
{
    public class ExecutionOrder
    {
        public string Id { get; set; } = default!;
        public string ExternalId { get; set; } = default!;
        public DateTime DateTime { get; set; }
        public string AssetId { get; set; } = default!;
        public string TradingAccount { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}