using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dba.DTO
{
    public class SoundEntry
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Category { get; set; }
        public virtual ICollection<SoundTag> SoundTags { get; set; } 
    }
}