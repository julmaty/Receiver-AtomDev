using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API2
{
    public class ReportTextContent
    {
        [Key] public int Id { get; set; }
        public int ReportId { get; set; }
        public string TextContent { get; set; }
    }
}
