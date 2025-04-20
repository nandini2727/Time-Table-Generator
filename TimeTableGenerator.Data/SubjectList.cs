using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTableGenerator.Data
{
    public class SubjectList
    {
        public int? SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int? PeriodsPerWeek { get; set; }
    }
}
