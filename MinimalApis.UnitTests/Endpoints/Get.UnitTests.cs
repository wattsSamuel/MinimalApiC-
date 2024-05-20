using Microsoft.EntityFrameworkCore;
using MinimalApi.Endpoints;
using MinimalApi.Models;

namespace MinimalApis.UnitTests.Endpoints;
public class Get
{
    [Fact]
    public async Task Get_ShouldThrowArgumentException_WhenParamsInvalid()
    {
        // Arrange
        var invalidId = 1;

        // Act
        Task Action() => new TodoItemsEndpoints().GetTodoItem(invalidId, new TodoContext(new DbContextOptions<TodoContext>()));

        // Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(Action);
    }
}
