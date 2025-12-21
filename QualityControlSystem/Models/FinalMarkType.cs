namespace QualityControlSystem.Models
{
    public class FinalMarkType
    {
        public int FinalMarkTypeId { get; set; }
        public string FinalMarkName { get; set; } = null!;

        // ОБРАТНЫЕ КОЛЛЕКЦИИ ПОЛНОСТЬЮ УДАЛЕНЫ
        // EF Core не может однозначно сопоставить три разные роли одной таблицы
        // Навигация работает только в одну сторону: Frame → FinalMarkType
    }
}