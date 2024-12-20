using System.Runtime.CompilerServices;
using FluentAssertions;
using DataAccessLibrary.Models;

namespace ToDoListApi.Tests.Models;

public class ToDoListItemTests
{

    [Fact]
    public void ItemToDo_CanBeAltered()
    {
        //Arrange
        var toDoObject = ToDoListDataBuilder.CreateToDoList(1, "Go for run", false);

        //Act
        toDoObject.ItemToDo = "Wipe counters";

        //Assert
        Assert.Equal("Wipe counters", toDoObject.ItemToDo);
    }

    [Fact]
    public void Completed_CanBeChanged()
    {
        //Arrange
        var toDoObject = ToDoListDataBuilder.CreateToDoList(1, "Wipe counters", false);

        //Act
        toDoObject.Completed = true;

        //Assert
        Assert.True(toDoObject.Completed);
    }

    [Theory]
    [InlineData(nameof(ToDoListModel.Id))]
    [InlineData(nameof(ToDoListModel.ItemToDo))]
    [InlineData(nameof(ToDoListModel.Completed))]
    public void ToDoListModel_ShouldHaveRequiredMemberAttributeSetOnAllProperties(string propertyName)
    {
        var requiredProperty = typeof(ToDoListModel).GetProperty(propertyName);

        requiredProperty.Should().BeDecoratedWith<RequiredMemberAttribute>();
    }
}
