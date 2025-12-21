using System.ComponentModel.DataAnnotations;

namespace QualityControlSystem.Models
{
    public class ProductionBatch
    {
        public int ProductionBatchId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? BatchStatusId { get; set; }

        public string? Recom { get; set; } // рекомендации эксперта

        // Навигационные свойства
        public BatchStatus? BatchStatus { get; set; }

        public ICollection<Frame> Frames { get; set; } = new List<Frame>();
    }
}