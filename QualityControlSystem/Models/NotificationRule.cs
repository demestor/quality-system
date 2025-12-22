namespace QualityControlSystem.Models
{
    public class NotificationRule
    {
        public int NotificationRuleId { get; set; }
        public int? SensorId { get; set; }
        public float? NormalValue { get; set; }
        public float? CriticalValue { get; set; }
        public int? NotificationTypeId { get; set; }
        public Sensor? Sensor { get; set; }
        public NotificationType? NotificationType { get; set; }
    }
}