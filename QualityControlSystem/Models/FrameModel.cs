namespace QualityControlSystem.Models
{
    public class FrameModel
    {
        public int FrameModelId { get; set; }
        public string FrameName { get; set; } = null!;

        public ICollection<Frame> Frames { get; set; } = new List<Frame>();
    }
}