using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Models;

namespace MinimalApi.Endpoints;

public class TodoItemsEndpoints : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/todo");
        group.MapPost("", PostTodoItem);
        group.MapGet("", GetTodoItems);
        group.MapGet("{id}", GetTodoItem).WithName(nameof(GetTodoItem));
        group.MapPut("{id}", PutTodoItem).WithName(nameof(PutTodoItem));
        group.MapDelete("{id}", DeleteTodoItem).WithName(nameof(DeleteTodoItem));
    }
    
    public async Task<IResult> GetTodoItems(
        TodoContext context)
    {
        return TypedResults.Ok( await context.TodoItems
            .Select(x => ItemToDTO(x))
            .ToListAsync());
    }
    
    public async Task<IResult> GetTodoItem(
        long id,
        TodoContext context)
    {
        var todoItem = await context.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(ItemToDTO(todoItem));
    }
    
    public async Task<IResult> PutTodoItem(long id,
        [FromBody]TodoItemDTO todoDTO,
        TodoContext context)
    {
        if (id != todoDTO.Id)
        {
            return TypedResults.BadRequest();
        }

        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return TypedResults.NotFound();
        }

        todoItem.Name = todoDTO.Name;
        todoItem.IsComplete = todoDTO.IsComplete;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TodoItemExists(id, context))
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
    
    public async Task<IResult> PostTodoItem(
        [FromBody]TodoItemDTO todoDTO,
        TodoContext context)
    {
        var todoItem = new TodoItem
        {
            IsComplete = todoDTO.IsComplete,
            Name = todoDTO.Name
        };

        context.TodoItems.Add(todoItem);
        await context.SaveChangesAsync();

        return TypedResults.CreatedAtRoute(nameof(GetTodoItem), new {id = todoItem.Id});
    }

    public async Task<IResult> DeleteTodoItem(long id, TodoContext context)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return TypedResults.NotFound();
        }

        context.TodoItems.Remove(todoItem);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private bool TodoItemExists(long id, TodoContext context)
    {
        return context.TodoItems.Any(e => e.Id == id);
    }

    private TodoItemDTO ItemToDTO(TodoItem todoItem) =>
       new TodoItemDTO
       {
           Id = todoItem.Id,
           Name = todoItem.Name,
           IsComplete = todoItem.IsComplete
       };
}