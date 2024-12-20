using FluentAssertions;
using ToDoList.WebApi.Validation;
using ToDoListApi.Tests;

namespace ToDoListApi.Tests.Validation;

public class ToDoListValidationTests
{
    [Theory]
    [InlineData(-1)]
    public void Validate_Id_CannotBeLessThan0(int id)
    {
        //Arrange
        var todoListDtoValidator = new ToDoListDtoValidator();
        var todoObject = ToDoListDataBuilder.CreateToDoList(id, "Eat", false);

        //Act
        var result = todoListDtoValidator.Validate(todoObject);

        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "'Id' must be greater than or equal to '0'.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData("       ")]
    [InlineData(null)]
    public void Validate_ItemToDo_CannotBeEmpty(string itemToDo)
    {
        var todoListDtoValidator = new ToDoListDtoValidator();
        var todoObject = ToDoListDataBuilder.CreateToDoList(1, itemToDo, false);

        var result = todoListDtoValidator.Validate(todoObject);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "'Item To Do' must not be empty.");
    }
}
