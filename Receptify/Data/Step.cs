using SQLite;

public class Step
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Description { get; set; }

    public int Order { get; set; }

    public int RecipeId { get; set; }
}
