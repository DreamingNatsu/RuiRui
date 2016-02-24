using System.ComponentModel.DataAnnotations;

namespace Dba.DTO
{
    public class Item
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string CustomHTML { get; set; }
    }
}