using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API2
{
    public class Report
    {
        [Key] public int Id { get; set; }
        public string? SenderName { get; set; }
        public string? ReportName { get; set; }
        public int? TextContent_Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateSent { get; set; }
        public DateTime? DateReceived { get; set; }
        public int? ReportDescription_Id { get; set; }
        public string? Filename { get; set; }
        public int Status { get; set; }
        public double Filesize { get; set; }
        public double BytesSend { get; set; }
    }
}
