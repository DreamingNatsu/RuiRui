using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dba.DTO
{
    public class SoundTag
    {
        public SoundTag()
        {
            this.SoundEntries = new HashSet<SoundEntry>();
        }
        [Key]
        public string Name { get; set; }
        [NotMapped]
        public virtual ICollection<SoundEntry> SoundEntries { get; set; } 
    }
}