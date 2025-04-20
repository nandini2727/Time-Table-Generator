using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTableGenerator.Data
{
    
    public class LessonList
    {
        
        public int? LessonID { get; set; }
        public int? SubjectID { get; set; }
        public int? TeacherID { get; set; }
        public int? ClassID { get; set; }
        public int? PeriodPerWeek { get; set; }
        public LessonList( int subjectId, int teacherId, int classId, int periodsPerWeek)
        {
            ClassID = classId;
            SubjectID = subjectId;
            TeacherID = teacherId;
            PeriodPerWeek = periodsPerWeek;
        }
    }
}
