using System.ComponentModel.DataAnnotations;

namespace Dba.DTO
{
    public class WebHome
    {
        [Key]
        public string WebHomeName { get; set; }
        public string JSon { get; set; }
    }

    public class WebImage
    {
        [Key]
        public string ImageId { get; set; }
        public string Path { get; set; }
    }
}