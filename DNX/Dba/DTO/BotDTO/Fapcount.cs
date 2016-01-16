using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class Fapcount
    {
        [Key]
        public int Id { get; set; }
        public string User { get; set; }
        public long Count { get; set; }
    }
}