using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTableGenerator.Data
{
    public class TeacherList
    {
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int? PeriodsCanBeAssigned { get; set; }
    }
}
