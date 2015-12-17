using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class JoinSbTrigger
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string SoundboardName { get; set; }
    }
}