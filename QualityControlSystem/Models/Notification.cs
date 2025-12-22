namespace QualityControlSystem.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int? SensorProdId { get; set; }
        public int? NotificationRuleId { get; set; }
        public int? FrameId { get; set; }
        public DateTime? NotificationTime { get; set; }

        // Навигации
        public ProdProcessedSensor? ProdProcessedSensor { get; set; }
        public NotificationRule? NotificationRule { get; set; }
        public Frame? Frame { get; set; }
    }
}