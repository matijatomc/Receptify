using SQLite;

public class Ingredient
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Text { get; set; }

    public int RecipeId { get; set; }
}
