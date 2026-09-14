namespace PizzaManagement.model
{
    public class PizzaInput
    {
        public float UserId { get; set; }
        public float PizzaId { get; set; }
        public string? TimeOfDay { get; set; }

        public float HasPineapple { get; set; }
        public float HasCheese { get; set; }
        public float HasSeafood { get; set; }
        public float SpicinessLevel { get; set; }
        public float CrustType { get; set; }
        public float PriceRange { get; set; }
        public float IsWeekend { get; set; } 
        public float IsPromotionActive { get; set; }
    }
}
