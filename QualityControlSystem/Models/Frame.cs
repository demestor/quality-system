using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace QualityControlSystem.Models
{
    public class Frame
    {
        public int FrameId { get; set; }

        public int? ProdBatchId { get; set; }

        public string? SerialNumber { get; set; }

        public int? FrameModelId { get; set; }

        // JSON-поле в базе (jsonb)
        public JsonDocument? VisualAnalysParams { get; set; }

        public int? SystemMarkId { get; set; }
        public int? ExpertMarkId { get; set; }
        public int? FinalMarkId { get; set; }

        // Навигационные свойства
        public ProductionBatch? Batch { get; set; }
        public FrameModel? FrameModel { get; set; }
        public FinalMarkType? SystemMark { get; set; }
        public FinalMarkType? ExpertMark { get; set; }
        public FinalMarkType? FinalMark { get; set; }

        // Только для форм Create/Edit
        [NotMapped]
        public string VisualJson { get; set; } = "{}";
    }
}