using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API2
{
    public class PeriodJsonModel
    {
        public double Speed { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

    }
}
