using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dba.DTO.Homepage;

namespace Dba.DTO.Homepage
{
    public sealed class TodoList
    {
        public TodoList(){
            this.TodoListItems = new List<TodoListItem>();
        }

        public int TodoListId { get; set; }
        public string Name { get; set; }
        public List<TodoListItem> TodoListItems { get; set; }
    }

    public class TodoListItem {
        public int TodoListItemId { get; set; }
        public string Description { get; set; }
        public int Importance { get; set; }
        public bool IsDone { get; set; }
        public int TodoListId { get; set; }
        public virtual TodoList TodoList { get; set; }
    }

}

namespace Dba.DAL {
    public partial class DbCtx {
        public DbSet<TodoList> TodoLists { get; set; }
        public DbSet<TodoListItem> TodoListItems { get; set; }
    }

}
