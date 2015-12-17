using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dba.DTO
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Item> Items { get; set; }
    }
}