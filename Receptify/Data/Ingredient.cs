using SQLite;

public class Ingredient
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
}
