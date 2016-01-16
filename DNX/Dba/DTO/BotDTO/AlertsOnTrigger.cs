using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class AlertsOnTrigger
    {
        [Key]
        public int Id { get; set; }
        public string Trigger { get; set; }
        public string User { get; set; }
        public bool IsRegex { get; set; }
        public string Message { get; set; }
    }
}