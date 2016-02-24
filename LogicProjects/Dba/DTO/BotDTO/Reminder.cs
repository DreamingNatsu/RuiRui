using System;
using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class Reminder
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CreatorId { get; set; }
        public string Message { get; set; }
        public string ChannelId { get; set; }
        public DateTime? Time { get; set; }
    }
}