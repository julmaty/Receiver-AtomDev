using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class PeriodTableModel
    {
        [Key] public int Id { get; set; }
        public double Speed { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
