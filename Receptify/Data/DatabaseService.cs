using SQLite;
using System.IO;

public class DatabaseService
{
    private static SQLiteAsyncConnection _database;

    public static async Task Init()
    {
        if (_database != null)
            return;

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "recipes.db");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<Recipe>();
        await _database.CreateTableAsync<Ingredient>();
        await _database.CreateTableAsync<Step>();
        await _database.CreateTableAsync<Tag>();
        await _database.CreateTableAsync<RecipeTag>();

    }

    public static Task<int> AddRecipeAsync(Recipe recipe) => _database.InsertAsync(recipe);

    public static Task<int> AddIngredientAsync(Ingredient ingredient) => _database.InsertAsync(ingredient);

    public static Task<int> AddStepAsync(Step step) => _database.InsertAsync(step);

    public static Task<int> AddTagAsync(Tag tag) => _database.InsertAsync(tag);

    public static async Task AddRecipeTagAsync(RecipeTag recipeTag)
    {
        await Init();
        await _database.InsertAsync(recipeTag);
    }

    public static async Task<List<Tag>> GetAllTagsAsync()
    {
        await Init();
        return await _database.Table<Tag>().ToListAsync();
    }
    public static async Task<Tag> GetTagByNameAsync(string name)
    {
        await Init();
        return await _database.Table<Tag>().Where(t => t.Name == name).FirstOrDefaultAsync();
    }
    public static async Task<List<Recipe>> GetAllRecipesAsync()
    {
        await Init();
        return await _database.Table<Recipe>().ToListAsync();
    }

    public static async Task<List<Tag>> GetTagsForRecipeAsync(int recipeId)
    {
        await Init();

        var tagIds = await _database.Table<RecipeTag>()
            .Where(rt => rt.RecipeId == recipeId)
            .ToListAsync();

        var tags = new List<Tag>();

        foreach (var rt in tagIds)
        {
            var tag = await _database.Table<Tag>().Where(t => t.Id == rt.TagId).FirstOrDefaultAsync();
            if (tag != null)
                tags.Add(tag);
        }

        return tags;
    }
    public static async Task<Recipe> GetRecipeByIdAsync(int id)
    {
        await Init();
        return await _database.Table<Recipe>().FirstOrDefaultAsync(r => r.Id == id);
    }

    public static async Task<List<Ingredient>> GetIngredientsByRecipeIdAsync(int recipeId)
    {
        await Init();
        return await _database.Table<Ingredient>().Where(i => i.RecipeId == recipeId).ToListAsync();
    }

    public static async Task<List<Step>> GetStepsByRecipeIdAsync(int recipeId)
    {
        await Init();
        return await _database.Table<Step>().Where(s => s.RecipeId == recipeId).ToListAsync();
    }
    public static async Task<int> UpdateRecipeAsync(Recipe recipe)
    {
        await Init();
        return await _database.UpdateAsync(recipe);
    }

    public static async Task<int> DeleteIngredientsByRecipeIdAsync(int recipeId)
    {
        await Init();
        var ingredients = await _database.Table<Ingredient>()
            .Where(i => i.RecipeId == recipeId)
            .ToListAsync();

        int deleted = 0;
        foreach (var item in ingredients)
        {
            deleted += await _database.DeleteAsync(item);
        }
        return deleted;
    }

    public static async Task<int> DeleteStepsByRecipeIdAsync(int recipeId)
    {
        await Init();
        var steps = await _database.Table<Step>()
            .Where(s => s.RecipeId == recipeId)
            .ToListAsync();

        int deleted = 0;
        foreach (var step in steps)
        {
            deleted += await _database.DeleteAsync(step);
        }
        return deleted;
    }

    public static async Task<int> DeleteRecipeTagsAsync(int recipeId)
    {
        await Init();
        var recipeTags = await _database.Table<RecipeTag>()
            .Where(rt => rt.RecipeId == recipeId)
            .ToListAsync();

        int deleted = 0;
        foreach (var tag in recipeTags)
        {
            deleted += await _database.DeleteAsync(tag);
        }
        return deleted;
    }

    public static async Task DeleteRecipeAsync(int recipeId)
    {
        await Init();
        var recipe = await _database.Table<Recipe>().Where(r => r.Id == recipeId).FirstOrDefaultAsync();
        if (recipe != null)
        {
            await _database.DeleteAsync(recipe);
        }
    }
}
