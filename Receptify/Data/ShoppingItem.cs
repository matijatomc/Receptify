using SQLite;

public class ShoppingItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Text { get; set; }
    public bool IsChecked { get; set; }
}

