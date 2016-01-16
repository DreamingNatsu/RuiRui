using System.ComponentModel.DataAnnotations;

namespace Dba.DTO
{
    public class RssTorrents
    {
        [Key]
        public int Id { get; set; }
        public string RssUrl { get; set; }
        public string Path { get; set; }
    }
}