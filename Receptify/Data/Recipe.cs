using SQLite;
using System.Collections.Generic;

public class Recipe
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; }
    public int CookingTimeMinutes { get; set; }
    public bool IsFavorite { get; set; }
    public int Rating { get; set; } = 0;
    public string Note { get; set; }

}
