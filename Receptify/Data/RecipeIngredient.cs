using SQLite;

public class RecipeIngredient
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public int IngredientId { get; set; }

    public double Quantity { get; set; }
}
