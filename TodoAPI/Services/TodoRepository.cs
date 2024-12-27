using Microsoft.AspNetCore.Identity;
using TodoAPI.Interfaces;
using TodoAPI.Models;

namespace TodoAPI.Services
{
    public class TodoRepository : ITodoRepository
    {
        //private readonly ApplicationDbContext _context;

        private List<TodoItem> _todoList;

        //public TodoRepository(ApplicationDbContext context)
        public TodoRepository()
        {
            InitializeData();
            //_context = context;
        }

        public IEnumerable<TodoItem> All
        {
            get { return _todoList; }
        }

        public bool DoesItemExist(string id)
        {
            return _todoList.Any(item => item.ID == id);
        }

        public TodoItem Find(string id)
        {
            return _todoList.FirstOrDefault(item => item.ID == id);
        }

        public void Insert(TodoItem item)
        {
            //item.ID = new Guid().ToString();
            _todoList.Add(item);
            //_context.TodoItems.Add(item);
            //await _context.SaveChangesAsync();
            //_context.SaveChanges();
        }

        public void Update(TodoItem item)
        {
            var todoItem = this.Find(item.ID);
            var index = _todoList.IndexOf(todoItem);
            _todoList.RemoveAt(index);
            _todoList.Insert(index, item);
        }

        public void Delete(string id)
        {
            _todoList.Remove(this.Find(id));
        }

        private void InitializeData()
        {
            _todoList = new List<TodoItem>();

            //_todoList = _context.TodoItems.ToList();

            var todoItem1 = new TodoItem
            {
                ID = "6bb8a868-dba1-4f1a-93b7-24ebce87e243",
                Name = "Learn app development",
                Notes = "Take Microsoft Learn Courses",
                Done = true
            };

            //var todoItem2 = new TodoItem
            //{
            //    ID = "b94afb54-a1cb-4313-8af3-b7511551b33b",
            //    Name = "Develop apps",
            //    Notes = "Use Visual Studio and Visual Studio for Mac",
            //    Done = false
            //};

            //var todoItem3 = new TodoItem
            //{
            //    ID = "ecfa6f80-3671-4911-aabe-63cc442c1ecf",
            //    Name = "Publish apps",
            //    Notes = "All app stores",
            //    Done = false,
            //};

            //var todoItem4 = new TodoItem
            //{
            //    ID = "4cfa6f80-3671-4911-aabe-63cc442c1ecf",
            //    Name = "4 Publish apps",
            //    Notes = "All app stores",
            //    Done = false,
            //};

            //var todoItem5 = new TodoItem
            //{
            //    ID = "5cfa6f80-3671-4911-aabe-63cc442c1ecf",
            //    Name = "5 Publish apps",
            //    Notes = "All app stores",
            //    Done = false,
            //};


            //var todoItem6 = new TodoItem
            //{
            //    ID = "6cfa6f80-3671-4911-aabe-63cc442c1ecf",
            //    Name = "6 Publish apps",
            //    Notes = "All app stores",
            //    Done = false,
            //};

            _todoList.Add(todoItem1);
            //_todoList.Add(todoItem2);
            //_todoList.Add(todoItem3);
            //_todoList.Add(todoItem4);
            //_todoList.Add(todoItem5);
            //_todoList.Add(todoItem6);

        }
    }
}
