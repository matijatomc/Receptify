﻿using SQLite;
using System.Collections.Generic;

public class Recipe
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; }
    public int CookingTimeMinutes { get; set; }
}
