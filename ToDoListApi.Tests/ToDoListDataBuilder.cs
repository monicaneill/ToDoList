using DataAccessLibrary.Models;

namespace ToDoListApi.Tests;

public class ToDoListDataBuilder
{
    public static ToDoListModel CreateToDoList(int id, string itemToDo, bool completed)
    {
        return new ToDoListModel
        {
            Id = id,
            ItemToDo = itemToDo,
            Completed = completed
        };
    }
}
