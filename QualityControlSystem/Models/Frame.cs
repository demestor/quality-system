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
        public JsonDocument? VisualAnalysParams { get; set; }
        public int? SystemMarkId { get; set; }
        public int? ExpertMarkId { get; set; }
        public int? FinalMarkId { get; set; }

        // Навигационные свойства (существующие + новые)
        public ProductionBatch? Batch { get; set; }
        public FrameModel? FrameModel { get; set; }
        public FinalMarkType? SystemMark { get; set; }
        public FinalMarkType? ExpertMark { get; set; }
        public FinalMarkType? FinalMark { get; set; }

        public ICollection<ProdProcessedSensor> ProdProcessedSensors { get; set; } = new List<ProdProcessedSensor>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}