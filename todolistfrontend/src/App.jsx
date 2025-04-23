import { useEffect, useState } from "react";

function App() {
    const [todoList, setTodoList] = useState([]);
    const [newTask, setNewTask] = useState("");

    const fetchTasks = async () => {
        const res = await fetch("https://localhost:7099/ToDo");
        const data = await res.json();
        setTodoList(data);
    };

    const addTask = async () => {
        if (newTask.trim() === "") return;

        const task = {
            id: 0,
            itemToDo: newTask,
            completed: false,
        };

        try {
            const response = await fetch("https://localhost:7099/ToDo", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(task),
            });

            if (response.ok) {
                fetchTasks();
                setNewTask("");
            } else {
                console.error("Add failed:", await response.text());
            }
        } catch (err) {
            console.error("Network error:", err);
        }
    };

    useEffect(() => {
        fetchTasks();
    }, []);

    return (
        <div className="App">
            <h1>ToDo List</h1>
            <input
                value={newTask}
                onChange={(e) => setNewTask(e.target.value)}
                placeholder="Add a task"
            />
            <button onClick={addTask}>Add Task</button>

            <ul>
                {todoList.map((task) => (
                    <li key={task.id}>
                        {task.itemToDo} {task.completed ? "✅" : ""}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default App;
