using SQLite;

public class RecipeTag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public int TagId { get; set; }
}
