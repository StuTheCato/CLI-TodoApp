using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TodoListApp
{
    class Program
    {
        
        private static string connectionString = "server=;user=;password=;database=;port=";

        static void Main(string[] args)
        {
            
            List<Todo> todos = new List<Todo>();
            int selectedIndex = 0;

            Console.WriteLine("Connect to the database, please wait...");
            try
            {
                todos = LoadTodosFromDatabase();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Sorry, the database is offline at the moment.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            ConsoleKeyInfo key;
            do
            {
                Console.Clear();
                DisplayTodos(todos, selectedIndex);
                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (selectedIndex > 0) selectedIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (selectedIndex < todos.Count - 1) selectedIndex++;
                        break;
                    case ConsoleKey.Spacebar:
                        if (todos.Count > 0)
                        {
                            todos[selectedIndex].IsCompleted = !todos[selectedIndex].IsCompleted;
                            UpdateTodoInDatabase(todos[selectedIndex]);
                        }
                        break;
                    case ConsoleKey.A:
                        Console.Clear();
                        Console.Write("Enter new todo: ");
                        string todoText = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(todoText))
                        {
                            Todo newTodo = new Todo { Text = todoText };
                            todos.Add(newTodo);
                            AddTodoToDatabase(newTodo);
                        }
                        break;
                    case ConsoleKey.R:
                        Console.Clear();
                        Console.Write("Enter index to remove or * to remove all: ");
                        string input = Console.ReadLine();
                        if (input == "*")
                        {
                            RemoveAllTodosFromDatabase();
                            todos.Clear();
                        }
                        else if (int.TryParse(input, out int index) && index >= 0 && index < todos.Count)
                        {
                            RemoveTodoFromDatabase(todos[index]);
                            todos.RemoveAt(index);
                            if (selectedIndex >= todos.Count) selectedIndex = todos.Count - 1;
                        }
                        break;
                }
            } while (key.Key != ConsoleKey.Q);
        }
        
        static void DisplayAsciiArt()
        {
            Console.WriteLine(@"
   _____ _____ _    _ 
  / ____|_   _| |  | |
 | (___   | | | |  | |
  \___ \  | | | |  | |
  ____) |_| |_| |__| |
 |_____/|_____|_____/ 
                     
   /\_/\  
  ( o.o ) 
   > ^ <
");
        }

        static List<Todo> LoadTodosFromDatabase()
        {
            List<Todo> todos = new List<Todo>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Todos";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    todos.Add(new Todo
                    {
                        Id = reader.GetInt32("Id"),
                        Text = reader.GetString("Text"),
                        IsCompleted = reader.GetBoolean("IsCompleted")
                    });
                }
            }

            return todos;
        }

        static void AddTodoToDatabase(Todo todo)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Todos (Text, IsCompleted) VALUES (@Text, @IsCompleted)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Text", todo.Text);
                cmd.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);
                cmd.ExecuteNonQuery();
                todo.Id = (int)cmd.LastInsertedId;
            }
        }

        static void UpdateTodoInDatabase(Todo todo)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Todos SET IsCompleted = @IsCompleted WHERE Id = @Id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);
                cmd.Parameters.AddWithValue("@Id", todo.Id);
                cmd.ExecuteNonQuery();
            }
        }

        static void RemoveTodoFromDatabase(Todo todo)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Todos WHERE Id = @Id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", todo.Id);
                cmd.ExecuteNonQuery();
            }
        }

        static void RemoveAllTodosFromDatabase()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Todos";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }

        static void DisplayTodos(List<Todo> todos, int selectedIndex)
        {
            DisplayAsciiArt();
            
            for (int i = 0; i < todos.Count; i++)
            {
                Todo todo = todos[i];
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                }
                if (todo.IsCompleted)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[x] ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ ] ");
                }
                Console.WriteLine(todo.Text);
                Console.ResetColor();
            }
            Console.WriteLine("\nUse Up/Down arrows to navigate, Space to toggle, 'a' to add, 'r' to remove, 'q' to quit.");
        }
    }

    class Todo
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
