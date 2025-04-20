using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTableGenerator.Data
{
    public class TimeTable
    {
        public int? TimeTableID { get; set; }
        public int? ClassID { get; set; }
        public string DayOfWeek { get; set; }
        public int? Period { get; set; }
        public int? SubjectID { get; set; }
        public int? TeacherID  { get; set; }
    }
}
