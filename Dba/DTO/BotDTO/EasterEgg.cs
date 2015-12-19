using System.ComponentModel.DataAnnotations;

namespace Dba.DTO.BotDTO {
    public class EasterEgg
    {
        [Key]
        public int Id { get; set; }
        public string Trigger { get; set; }
        public EasterEggType TriggerType { get; set; }
        public ResponseType ResponseType { get; set; }
        public string ResponseFormat { get; set; }
        public bool IsActive { get; set; }
    }
    public enum EasterEggType
    {
        Equals,Contains,Regex,
        EqualsCheckCase,ContainsCheckCase,
        MultiMessage
    }

    public enum ResponseType
    {
        String,Eval
    }
}