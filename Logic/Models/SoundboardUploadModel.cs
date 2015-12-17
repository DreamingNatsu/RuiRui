using System.Collections.Generic;
using System.ComponentModel;
using System.Web;

namespace Logic.Models
{
    public class UploadViewModel
    {
        [DisplayName("Select Files to Upload")]
        public IEnumerable<HttpPostedFileBase> Files { get; set; }

        public string Category { get; set; }
    }
}