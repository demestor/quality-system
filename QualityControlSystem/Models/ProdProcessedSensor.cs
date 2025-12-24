namespace QualityControlSystem.Models
{
    public class ProdProcessedSensor
    {
        public int ProdProcessedSensorId { get; set; }
        public int? SensorId { get; set; }
        public int? FrameId { get; set; }
        public float? ValueAfterProc { get; set; }
        public DateTime? ProcessTime { get; set; }
        public Sensor? Sensor { get; set; }
        public Frame? Frame { get; set; }
    }
}