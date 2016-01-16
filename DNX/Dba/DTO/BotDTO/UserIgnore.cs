using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class UserIgnore
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
    }
}