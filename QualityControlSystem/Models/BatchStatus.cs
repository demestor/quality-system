namespace QualityControlSystem.Models
{
    public class BatchStatus
    {
        public int BatchStatusId { get; set; }
        public string StatusName { get; set; } = null!;

        public ICollection<ProductionBatch> ProductionBatches { get; set; } = new List<ProductionBatch>();
    }
}