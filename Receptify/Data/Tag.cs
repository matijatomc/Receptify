using SQLite;
using System.ComponentModel;

public class Tag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }

    public bool IsSelected { get; set; }

}
