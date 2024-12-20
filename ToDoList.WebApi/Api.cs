using FluentValidation;

namespace ToDoList.WebApi;

public static class Api
{
    public static void ConfigureApi(this WebApplication app) //extension method
    {
        // all of my api endpoint mapping
        app.MapGet("/ToDo", GetToDoListItems);
        app.MapGet("/ToDo/id/{id}", GetToDoListItem);
        app.MapGet("/ToDo/completed/{completed}", FilterToDoListItem);
        app.MapPost("/ToDo", InsertToDoItem);
        app.MapPut("/ToDo", UpdateToDoItem);
        app.MapDelete("/ToDo/id/{id}", DeleteToDoItem);
    }

    public static async Task<IResult> GetToDoListItems(IToDoListData data)
    {
        try
        {
            var items = await data.GetToDoListItems();
            if (items == null || !items.Any())
            {
                return Results.NotFound("No to-do list items are present, try adding some!");
            }
            return Results.Ok(items);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    public static async Task<IResult> GetToDoListItem(int id, IToDoListData data)
    {
        try
        {
            var results = await data.GetToDoListItem(id);
            return results is null ? Results.NotFound() : Results.Ok(results);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    /// <summary>
    /// Gets the to do list item based on completion status true/false
    /// </summary>
    /// <param name="completed"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<IResult> FilterToDoListItem(bool completed, IToDoListData data)
    {
        try
        {
            var results = await data.FilterToDoListItem(completed);

            if (results == null || !results.Any())
            {
                return Results.NotFound();
            }

            return Results.Ok(results);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    /// <summary>
    /// Inserts a new to do list item
    /// </summary>
    /// <param name="toDoList">When inserting a new item, don't alter the id field (leave as 0). This will automatically insert as the next auto-increment id</param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<IResult> InsertToDoItem(ToDoListModel toDoList, IToDoListData data, IValidator<ToDoListModel> validator)
    {
        var validationResult = await validator.ValidateAsync(toDoList);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            await data.InsertToDoItem(toDoList);
            return Results.Ok();
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    /// <summary>
    /// Updates an item by overwriting the id with the data changes desired
    /// </summary>
    /// <param name="toDoList">Update the model below with the data desired</param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<IResult> UpdateToDoItem(ToDoListModel toDoList, IToDoListData data, IValidator<ToDoListModel> validator)
    {
        var validationResult = await validator.ValidateAsync(toDoList);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            await data.UpdateToDoItem(toDoList);
            return Results.Ok();
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    /// <summary>
    /// Deletes individual to do list items by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<IResult> DeleteToDoItem(int id, IToDoListData data)
    {
        try
        {
            var existingItem = await data.GetToDoListItem(id);
            if (existingItem == null)
            {
                return Results.NotFound();
            }

            await data.DeleteToDoItem(id);
            return Results.Ok();
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }
}