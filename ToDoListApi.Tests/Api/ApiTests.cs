using DataAccessLibrary.Data;
using DataAccessLibrary.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using ApiClass = ToDoList.WebApi.Api;

namespace ToDoListApi.Tests.Api;

public class ApiTests
{
    #region GetTests

    [Fact]
    public async Task GetToDoListItems_ReturnsOkResult_WithMultipleToDoListItems()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();
        mockData.Setup(data => data.GetToDoListItems())
            .ReturnsAsync(new List<ToDoListModel>
            {
                new() { Id = 1, ItemToDo = "Test Task 1", Completed = false },
                new() { Id = 2, ItemToDo = "Test Task 2", Completed = true },
                new() { Id = 3, ItemToDo = "Test Task 3", Completed = false }
            });

        // Act
        var result = await ApiClass.GetToDoListItems(mockData.Object);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<ToDoListModel>>>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ToDoListModel>>(okResult.Value);

        Assert.Equal(3, returnValue.Count());
        Assert.Equal("Test Task 1", returnValue.ElementAt(0).ItemToDo);
        Assert.Equal("Test Task 2", returnValue.ElementAt(1).ItemToDo);
        Assert.Equal("Test Task 3", returnValue.ElementAt(2).ItemToDo);
    }

    [Fact]
    public async Task GetToDoListItem_ReturnsOkResult_WithToDoListItemId()
    {
        // Arrange
        int testId = 1;
        var mockData = new Mock<IToDoListData>();
        mockData.Setup(data => data.GetToDoListItem(testId))
            .ReturnsAsync(new ToDoListModel { Id = testId, ItemToDo = "Test Task", Completed = false });

        // Act
        var result = await ApiClass.GetToDoListItem(testId, mockData.Object);

        // Assert
        var okResult = Assert.IsType<Ok<ToDoListModel>>(result);
        var returnValue = Assert.IsType<ToDoListModel>(okResult.Value);
        Assert.Equal(testId, returnValue.Id);
        Assert.Equal("Test Task", returnValue.ItemToDo);
    }

    [Fact]
    public async Task GetToDoListItems_ReturnsNotFoundWithMessage_WhenNoItemsPresent()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();
        mockData.Setup(data => data.GetToDoListItems())
            .ReturnsAsync(new List<ToDoListModel>()); // Return an empty list

        // Act
        var result = await ApiClass.GetToDoListItems(mockData.Object);

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("No to-do list items are present, try adding some!", notFoundResult.Value);
    }

    [Fact]
    public async Task GetToDoListItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        int testId = 1;
        var mockData = new Mock<IToDoListData>();
        mockData.Setup(data => data.GetToDoListItem(testId))!
            .ReturnsAsync((ToDoListModel)null!);

        // Act
        var result = await ApiClass.GetToDoListItem(testId, mockData.Object);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    #endregion

    #region FilterTests

    [Fact]
    public async Task FilterToDoListItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        bool completed = true;
        var mockData = new Mock<IToDoListData>();
        mockData.Setup(data => data.FilterToDoListItem(completed))!
            .ReturnsAsync((IEnumerable<ToDoListModel>)null!);

        // Act
        var result = await ApiClass.FilterToDoListItem(completed, mockData.Object);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task FilterToDoListItem_ReturnsItemByCompletedBool()
    {
        // Arrange
        bool completed = true;
        var mockData = new Mock<IToDoListData>();

        mockData.Setup(data => data.FilterToDoListItem(completed))
            .ReturnsAsync(new List<ToDoListModel>
            {
                new() { Id = 1, ItemToDo = "Task 1", Completed = true },
                new() { Id = 2, ItemToDo = "Task 2", Completed = true },
                new() { Id = 3, ItemToDo = "Task 3", Completed = false }
            });

        // Act
        var result = await ApiClass.FilterToDoListItem(completed, mockData.Object);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<ToDoListModel>>>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ToDoListModel>>(okResult.Value);

        var filteredResults = returnValue.Where(item => item.Completed == completed);

        Assert.All(filteredResults, item => Assert.True(item.Completed));
        Assert.Equal(2, filteredResults.Count());
    }

    #endregion

    #region InsertTests

    [Fact]
    public async Task InsertToDoItem_ReturnsOkResult_WhenItemIsInsertedSuccessfully()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();
        var newToDoItem = new ToDoListModel { Id = 0, ItemToDo = "New Task", Completed = false };

        mockData.Setup(data => data.InsertToDoItem(newToDoItem))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var mockValidator = new Mock<IValidator<ToDoListModel>>();
        mockValidator.Setup(v => v.ValidateAsync(newToDoItem, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await ApiClass.InsertToDoItem(newToDoItem, mockData.Object, mockValidator.Object);

        // Assert
        Assert.IsType<Ok>(result);
        mockData.Verify(data => data.InsertToDoItem(newToDoItem), Times.Once);
    }

    [Fact]
    public async Task InsertToDoItem_DoesNotAddWhiteSpaceOrNull()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();
        var mockValidator = new Mock<IValidator<ToDoListModel>>();

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ToDoListModel>(), default))
            .ReturnsAsync((ToDoListModel model, CancellationToken _) =>
            {
                var validationResult = new ValidationResult();

                if (string.IsNullOrWhiteSpace(model.ItemToDo))
                {
                    validationResult.Errors.Add(new ValidationFailure("ItemToDo", "'Item To Do' must not be empty."));
                }

                return validationResult;
            });

        var invalidItems = new List<string>
        {
            "",
            " ",
            "\n",
            "\r",
            "\r\n",
            "       ",
            null
        };

        foreach (var invalidItem in invalidItems)
        {
            var newItem = new ToDoListModel { Id = 0, ItemToDo = invalidItem, Completed = false };

            // Act
            var result = await ApiClass.InsertToDoItem(newItem, mockData.Object, mockValidator.Object);

            // Assert
            Assert.IsType<ProblemHttpResult>(result);

            mockData.Verify(data => data.InsertToDoItem(It.IsAny<ToDoListModel>()), Times.Never());
        }
    }

    [Fact]
    public async Task InsertToDoItem_ReturnsProblemResult_WhenExceptionIsThrown()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();
        var newToDoItem = new ToDoListModel { Id = 0, ItemToDo = "New Task", Completed = false };

        mockData.Setup(data => data.InsertToDoItem(newToDoItem))
            .ThrowsAsync(new Exception("Database error"));

        var mockValidator = new Mock<IValidator<ToDoListModel>>();

        mockValidator.Setup(v => v.ValidateAsync(newToDoItem, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await ApiClass.InsertToDoItem(newToDoItem, mockData.Object, mockValidator.Object);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
    }

    #endregion

    #region UpdateTests

    [Fact]
    public async Task UpdateToDoItem_OverwritesExistingItem()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();

        var existingToDoItem = new ToDoListModel { Id = 1, ItemToDo = "Pre-Existing Item", Completed = false };

        var updatedToDoItem = new ToDoListModel { Id = 1, ItemToDo = "Updated Task", Completed = true };

        mockData.Setup(data => data.GetToDoListItem(1))
            .ReturnsAsync(existingToDoItem);

        mockData.Setup(data => data.UpdateToDoItem(updatedToDoItem))
            .Returns(Task.CompletedTask)
            .Verifiable();

        mockData.Setup(data => data.GetToDoListItem(1))
            .ReturnsAsync(updatedToDoItem);

        var mockValidator = new Mock<IValidator<ToDoListModel>>();
        mockValidator.Setup(v => v.ValidateAsync(updatedToDoItem, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        await ApiClass.UpdateToDoItem(updatedToDoItem, mockData.Object, mockValidator.Object);

        var result = await mockData.Object.GetToDoListItem(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedToDoItem.ItemToDo, result.ItemToDo);
        Assert.True(result.Completed);

        mockData.Verify(data => data.UpdateToDoItem(updatedToDoItem), Times.Once);

        Assert.NotEqual(existingToDoItem.ItemToDo, result.ItemToDo);
    }

    #endregion

    #region DeleteTests

    [Fact]
    public async Task DeleteToDoItem_ReturnsOkResult_WhenDeletionIsSuccessful()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();

        var existingToDoItem = new ToDoListModel { Id = 1, ItemToDo = "Task to delete", Completed = false };
        mockData.Setup(data => data.GetToDoListItem(1))
            .ReturnsAsync(existingToDoItem);

        mockData.Setup(data => data.DeleteToDoItem(1))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await ApiClass.DeleteToDoItem(1, mockData.Object);

        // Assert
        Assert.IsType<Ok>(result);
        mockData.Verify(data => data.GetToDoListItem(1), Times.Once);
        mockData.Verify(data => data.DeleteToDoItem(1), Times.Once);
    }

    [Fact]
    public async Task DeleteToDoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();

        mockData.Setup(data => data.GetToDoListItem(999))
            .ReturnsAsync((ToDoListModel)null);

        // Act
        var result = await ApiClass.DeleteToDoItem(999, mockData.Object);

        // Assert
        Assert.IsType<NotFound>(result);
        mockData.Verify(data => data.GetToDoListItem(999), Times.Once);
        mockData.Verify(data => data.DeleteToDoItem(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteToDoItem_ReturnsProblem_WhenExceptionIsThrown()
    {
        // Arrange
        var mockData = new Mock<IToDoListData>();

        var existingToDoItem = new ToDoListModel { Id = 1, ItemToDo = "Task to delete", Completed = false };
        mockData.Setup(data => data.GetToDoListItem(1))
            .ReturnsAsync(existingToDoItem);

        mockData.Setup(data => data.DeleteToDoItem(1))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await ApiClass.DeleteToDoItem(1, mockData.Object);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal("Database error", problemResult.ProblemDetails.Detail);

        mockData.Verify(data => data.GetToDoListItem(1), Times.Once); 
        mockData.Verify(data => data.DeleteToDoItem(1), Times.Once);
    }

    #endregion
}