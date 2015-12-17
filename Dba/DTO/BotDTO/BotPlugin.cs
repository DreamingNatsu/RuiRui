using System.ComponentModel.DataAnnotations.Schema;

namespace Dba.DTO.BotDTO {
    [ComplexType]
    public class PersistableLongCollection : PersistableScalarCollection<long>
    {
        protected override long ConvertSingleValueToRuntime(string rawValue)
        {
            return long.Parse(rawValue);
        }
        protected override string ConvertSingleValueToPersistable(long value)
        {
            return value.ToString();
        }
    }
    public class BotPlugin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Developer { get; set; }
        public BotFilterType FilterType { get; set; }
        public PersistableLongCollection Whitelist { get; set; }
        public PersistableLongCollection Blacklist { get; set; }
        public bool Disabled { get; set; }
    }
    public enum BotFilterType
    {
        Whitelist, Blacklist, Unfiltered, Locked
    }
}