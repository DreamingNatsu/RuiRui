using System;
using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class CheckTimer
    {
        [Key]
        public int Id { get; set; }
        public string Identifier { get; set; }
        public DateTime DateTime { get; set; }
    }
}