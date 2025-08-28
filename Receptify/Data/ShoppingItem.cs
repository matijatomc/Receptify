using SQLite;

public class ShoppingItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int IngredientId { get; set; }

    public string Name { get; set; }
    public string Unit { get; set; }
    public double Quantity { get; set; }
    public bool IsChecked { get; set; }

    public string DisplayText => $"{Quantity} {Unit} {Name}";
}
