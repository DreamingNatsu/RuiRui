using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dba.DAL;
using Dba.DTO.Homepage;

namespace Logic.Homepage
{
    class TodoManager : IDisposable {
        private DbCtx _db;
        public TodoManager(){
            _db = new DbCtx();
        }

        public TodoList GetTodoList(int todoListId){
            return _db.TodoLists.Where(td => td.TodoListId == todoListId).Include(i => i.TodoListItems).FirstOrDefault();
        }

        public void SetTodoList(TodoList todoListIn){
            var tdl = _db.TodoLists.Find(todoListIn.TodoListId);
            if (tdl == null) throw new NullReferenceException();
            
            todoListIn.TodoListItems.ForEach(i =>{
                if (tdl.TodoListItems.Any(j=>j.TodoListItemId==i.TodoListItemId)) {
                    _db.TodoListItems.Add(i);
                    tdl.TodoListItems.Add(i);
                }
                else {
                    _db.TodoListItems.Attach(i);
                    _db.Entry(i).State= EntityState.Modified;
                }
            });
            _db.SaveChanges();
        }

        public void AddTodoList(TodoList todoList){
            _db.TodoLists.Add(todoList);
            _db.SaveChanges();
        }

        public void Dispose(){
            _db.Dispose();
        }
    }
}
