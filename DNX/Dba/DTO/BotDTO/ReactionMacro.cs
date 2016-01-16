using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class ReactionMacro
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Identifier { get; set; }
    }
    public class ReactionMacroType
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}