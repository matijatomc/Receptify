using SQLite;
using System.IO;
using Receptify.Models;

public static class DatabaseService
{
    private static SQLiteAsyncConnection _database;

    public static async Task Init()
    {
        if (_database != null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recipes.db");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<Recipe>();
        await _database.CreateTableAsync<Ingredient>();
        await _database.CreateTableAsync<Step>();
        await _database.CreateTableAsync<Tag>();
        await _database.CreateTableAsync<RecipeTag>();
        await _database.CreateTableAsync<ShoppingItem>();
    }

    // -------------------- RECIPE --------------------
    public static async Task<int> AddRecipeAsync(Recipe recipe)
    {
        await Init();
        return await _database.InsertAsync(recipe);
    }

    public static async Task<Recipe> GetRecipeByIdAsync(int id)
    {
        await Init();
        return await _database.Table<Recipe>().FirstOrDefaultAsync(r => r.Id == id);
    }

    public static async Task<List<Recipe>> GetAllRecipesAsync()
    {
        await Init();
        return await _database.Table<Recipe>().ToListAsync();
    }

    public static async Task<int> UpdateRecipeAsync(Recipe recipe)
    {
        await Init();
        return await _database.UpdateAsync(recipe);
    }

    public static async Task DeleteRecipeAsync(int recipeId)
    {
        await Init();
        var recipe = await GetRecipeByIdAsync(recipeId);
        if (recipe != null)
        {
            await _database.DeleteAsync(recipe);
        }
    }

    public static async Task ToggleFavoriteAsync(int recipeId, bool isFavorite)
    {
        await Init();
        var recipe = await _database.Table<Recipe>().Where(r => r.Id == recipeId).FirstOrDefaultAsync();
        if (recipe != null)
        {
            recipe.IsFavorite = isFavorite;
            await _database.UpdateAsync(recipe);
        }
    }

    // -------------------- INGREDIENT --------------------
    public static async Task<int> AddIngredientAsync(Ingredient ingredient)
    {
        await Init();
        return await _database.InsertAsync(ingredient);
    }

    public static async Task<List<Ingredient>> GetIngredientsByRecipeIdAsync(int recipeId)
    {
        await Init();
        return await _database.Table<Ingredient>().Where(i => i.RecipeId == recipeId).ToListAsync();
    }

    public static async Task<int> DeleteIngredientsByRecipeIdAsync(int recipeId)
    {
        await Init();
        var ingredients = await GetIngredientsByRecipeIdAsync(recipeId);
        int deleted = 0;

        foreach (var ing in ingredients)
            deleted += await _database.DeleteAsync(ing);

        return deleted;
    }

    // -------------------- STEP --------------------
    public static async Task<int> AddStepAsync(Step step)
    {
        await Init();
        return await _database.InsertAsync(step);
    }

    public static async Task<List<Step>> GetStepsByRecipeIdAsync(int recipeId)
    {
        await Init();
        return await _database.Table<Step>().Where(s => s.RecipeId == recipeId).ToListAsync();
    }

    public static async Task<int> DeleteStepsByRecipeIdAsync(int recipeId)
    {
        await Init();
        var steps = await GetStepsByRecipeIdAsync(recipeId);
        int deleted = 0;

        foreach (var step in steps)
            deleted += await _database.DeleteAsync(step);

        return deleted;
    }

    // -------------------- TAG --------------------
    public static async Task<int> AddTagAsync(Tag tag)
    {
        await Init();
        return await _database.InsertAsync(tag);
    }

    public static async Task<List<Tag>> GetAllTagsAsync()
    {
        await Init();
        return await _database.Table<Tag>().ToListAsync();
    }

    public static async Task<Tag> GetTagByNameAsync(string name)
    {
        await Init();
        return await _database.Table<Tag>().FirstOrDefaultAsync(t => t.Name == name);
    }

    public static async Task<Tag> GetTagByIdAsync(int id)
    {
        await Init();
        return await _database.Table<Tag>().FirstOrDefaultAsync(t => t.Id == id);
    }

    public static async Task<int> UpdateTagAsync(Tag tag)
    {
        await Init();
        return await _database.UpdateAsync(tag);
    }

    public static async Task DeleteTagAsync(int tagId)
    {
        await Init();
        await _database.DeleteAsync<Tag>(tagId);
    }

    // -------------------- RECIPE-TAG --------------------
    public static async Task AddRecipeTagAsync(RecipeTag recipeTag)
    {
        await Init();
        await _database.InsertAsync(recipeTag);
    }

    public static async Task<List<Tag>> GetTagsForRecipeAsync(int recipeId)
    {
        await Init();
        var tagLinks = await _database.Table<RecipeTag>()
            .Where(rt => rt.RecipeId == recipeId).ToListAsync();

        var tags = new List<Tag>();
        foreach (var rt in tagLinks)
        {
            var tag = await GetTagByIdAsync(rt.TagId);
            if (tag != null)
                tags.Add(tag);
        }
        return tags;
    }

    public static async Task<int> DeleteRecipeTagsAsync(int recipeId)
    {
        await Init();
        var recipeTags = await _database.Table<RecipeTag>()
            .Where(rt => rt.RecipeId == recipeId).ToListAsync();

        int deleted = 0;
        foreach (var rt in recipeTags)
            deleted += await _database.DeleteAsync(rt);

        return deleted;
    }

    // -------------------- SHOPPING ITEM --------------------
    public static async Task<int> AddShoppingItemAsync(ShoppingItem item)
    {
        await Init();
        return await _database.InsertAsync(item);
    }

    public static async Task<List<ShoppingItem>> GetShoppingItemsAsync()
    {
        await Init();
        return await _database.Table<ShoppingItem>().ToListAsync();
    }

    public static async Task DeleteShoppingItemAsync(int id)
    {
        await Init();
        await _database.DeleteAsync<ShoppingItem>(id);
    }

    public static async Task DeleteAllShoppingItemsAsync()
    {
        await Init();
        await _database.ExecuteAsync("DELETE FROM ShoppingItem");
    }
}
