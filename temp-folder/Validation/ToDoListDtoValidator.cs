using FluentValidation;

namespace ToDoList.WebApi.Validation;

public class ToDoListDtoValidator : AbstractValidator<ToDoListModel>
{
    public ToDoListDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ItemToDo)
            .MinimumLength(1)
            .NotEmpty();
    }
}
