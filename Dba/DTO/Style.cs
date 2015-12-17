using System.ComponentModel.DataAnnotations;

namespace Dba.DTO
{
    public class Style
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
        public string CSS { get; set; }
        public string CategoryHTML { get; set; }
        public string ItemHTML { get; set; }
        public string PageHTML { get; set; }
    }
}