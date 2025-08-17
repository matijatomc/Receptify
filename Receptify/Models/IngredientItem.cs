namespace Receptify.Models
{
    public class IngredientItem
    {
        public string Name { get; set; }

        public double Quantity { get; set; }

        public string Unit { get; set; }

        public int? IngredientId { get; set; }

        public string DisplayText => $"{Quantity} {Unit} {Name}";
    }
}
