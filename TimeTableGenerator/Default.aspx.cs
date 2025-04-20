using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using TimeTableGenerator.Data;

namespace TimeTableGenerator
{
    public partial class Default : System.Web.UI.Page
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBCS"].ToString();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateClassDropdown();
            }
            
                int selectedClassId = Convert.ToInt32(ddlClass.SelectedValue);
                PopulateSubjectsForClass(selectedClassId);
            
        }

        private void PopulateClassDropdown()
        {
            string query = "SELECT ClassID, ClassName FROM tblClassList";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlClass.DataSource = reader;
                ddlClass.DataTextField = "ClassName";
                ddlClass.DataValueField = "ClassID";
                ddlClass.DataBind();
            }
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedClassId = Convert.ToInt32(ddlClass.SelectedValue);
            PopulateSubjectsForClass(selectedClassId);
        }

        private void PopulateSubjectsForClass(int classId)
        {
            string query = "SELECT SubjectID, SubjectName FROM tblSubjectList ";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClassID", classId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                subjectsContainer.Controls.Clear(); 

                while (reader.Read())
                {
                    Label lblSubject = new Label();
                    lblSubject.Text = reader["SubjectName"].ToString();
                    subjectsContainer.Controls.Add(lblSubject);

                    // TextBox for Periods per week
                    TextBox txtPeriods = new TextBox();
                    txtPeriods.ID = "txtPeriods_" + reader["SubjectID"];
                    txtPeriods.Width = 100;
                    txtPeriods.Attributes["placeholder"] = "Periods/week";
                    subjectsContainer.Controls.Add(txtPeriods);

                    // Dropdown for Teacher selection
                    DropDownList ddlTeachers = new DropDownList();
                    ddlTeachers.ID = "ddlTeacher_" + reader["SubjectID"];
                    ddlTeachers.Width = 200;
                    ddlTeachers.DataSource = GetAvailableTeachersForClass(classId);
                    ddlTeachers.DataTextField = "TeacherName";
                    ddlTeachers.DataValueField = "TeacherID";
                    ddlTeachers.DataBind();
                    subjectsContainer.Controls.Add(ddlTeachers);

                    // RadioButton for selecting the class teacher for each subject
                    RadioButton rblClassTeacher = new RadioButton();
                    rblClassTeacher.ID = "rblClassTeacher_" + reader["SubjectID"];
                    rblClassTeacher.GroupName = "ClassTeacherGroup";  
                    rblClassTeacher.Text = "Class Teacher";
                    rblClassTeacher.AutoPostBack = true;  
                    subjectsContainer.Controls.Add(rblClassTeacher);

                    subjectsContainer.Controls.Add(new Literal { Text = "<br /><br />" });
                }
            }
        }

        protected void btnGenerateTimetable_Click(object sender, EventArgs e)
        {
            int classId = Convert.ToInt32(ddlClass.SelectedValue);
            var subjects = new List<SubjectList>();

            foreach (Control ctrl in subjectsContainer.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    string[] parts = txt.ID.Split('_');
                    int subjectId = Convert.ToInt32(parts[1]);

                    int periodsPerWeek = 0;
                    if (int.TryParse(txt.Text, out periodsPerWeek) && periodsPerWeek > 0)
                    {
                        subjects.Add(new SubjectList
                        {
                            SubjectId = subjectId,
                            PeriodsPerWeek = periodsPerWeek
                        });
                    }
                }
            }

            // Fetch the available teachers
            var teachers = GetAvailableTeachersForClass(classId);

            // Now generate the timetable
            var timetable = GenerateTimetable(classId, subjects, teachers);

            // Optionally, save the timetable to the database
            SaveTimetableToDatabase(timetable);

            Response.Write("<script>alert('Timetable generated successfully');</script>");
        }

        private List<TeacherList> GetAvailableTeachersForClass(int classId)
        {
            var teachers = new List<TeacherList>();

            string query = "SELECT TeacherID, TeacherName, PeriodsCanBeAssigned FROM tblTeacherList";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    teachers.Add(new TeacherList
                    {
                        TeacherId = Convert.ToInt32(reader["TeacherID"]),
                        TeacherName = reader["TeacherName"].ToString(),
                        PeriodsCanBeAssigned = Convert.ToInt32(reader["PeriodsCanBeAssigned"])
                    });
                }
            }
            return teachers;
        }

        private List<TimeTable> GenerateTimetable(int classId, List<SubjectList> subjects, List<TeacherList> teachers)
        {
            var timetable = new List<TimeTable>();

            // Define days and periods
            var daysOfWeek = new List<string> { "1", "2", "3", "4", "5","6"};
            int totalSlotsPerDay = 8; // 8 periods per day

            // Create empty timetable slots for the whole week (5 days, 8 periods each day)
                for (int periodIndex = 1; periodIndex <= totalSlotsPerDay; periodIndex++)
                {
                for (int dayIndex = 0; dayIndex < daysOfWeek.Count; dayIndex++)
                {
                    timetable.Add(new TimeTable
                    {
                        TimeTableID = dayIndex * totalSlotsPerDay + periodIndex,
                        ClassID = classId,
                        SubjectID = -1, // No subject assigned yet
                        TeacherID = -1, // No teacher assigned yet
                        DayOfWeek = daysOfWeek[dayIndex],
                        Period = periodIndex
                    });
                }
                }


            // Assign the class teacher to the first period on any day (let's choose Monday, 1st period)
            var classTeacherId = -1;
            foreach (var subject in subjects)
            {
                // Find the class teacher (assuming the class teacher selection is a radio button)
                var teacherId = Convert.ToInt32(((DropDownList)subjectsContainer.FindControl("ddlTeacher_" + subject.SubjectId)).SelectedValue); // Get selected teacher
                var isClassTeacher = ((RadioButton)subjectsContainer.FindControl("rblClassTeacher_" + subject.SubjectId)).Checked;

                if (isClassTeacher)
                {
                    classTeacherId = teacherId;

                    // Assign the class teacher to the first period on the first day (Monday 1st period)
                    var firstPeriodSlot = timetable.Where(slot =>  slot.Period == 1 && slot.SubjectID == -1 && slot.TeacherID == -1).ToList();
                    if (firstPeriodSlot.Count > 0)
                    {
                        foreach (var slot in firstPeriodSlot) {
                            slot.SubjectID = subject.SubjectId;
                            slot.TeacherID = classTeacherId;
                        }
                        
                    }
                    
                }
            }
            // Assign subjects to slots (start from Monday 1st period, then Tuesday 1st period, etc.)

            foreach (var subject in subjects)
            {
                int assignedPeriods = 0;
                var teacherId = Convert.ToInt32(((DropDownList)subjectsContainer.FindControl("ddlTeacher_" + subject.SubjectId)).SelectedValue); // Get selected teacher

                if (teacherId == classTeacherId)
                {
                    continue; // Skip this subject if it's the class teacher since we've already assigned their first period
                }

                Random rand = new Random(); // Create the random number generator once, not inside the loop

                // Keep trying to assign periods until all required periods are assigned
                while (assignedPeriods < subject.PeriodsPerWeek)
                {
                    // Try to find a free slot for the subject
                    bool periodAssigned = false;

                    // List of available slots
                    var availableSlots = timetable.Where(slot => slot.SubjectID == -1 && slot.TeacherID == -1 && IsTeacherAvailableForPeriod(slot.DayOfWeek, slot.Period, teacherId)).ToList();

                    if (availableSlots.Count > 0)
                    {
                        // Pick a random available slot
                        var randomSlot = availableSlots[rand.Next(availableSlots.Count)];

                        // Assign the subject and teacher to this random slot
                        randomSlot.SubjectID = subject.SubjectId;
                        randomSlot.TeacherID = teacherId;

                        assignedPeriods++;
                        periodAssigned = true;
                    }

                    // If no period was assigned, break the loop (to prevent infinite loops if no slots are available)
                    if (!periodAssigned)
                    {
                        // Handle this case (e.g., log an error, notify the user, or attempt again)
                        break;
                    }
                }
            }

            return timetable;
        }

        private bool IsTeacherAvailableForPeriod(string day, int? period, int teacherId)
        {
            string query = "select count(*) from tblTimeTable where DayOfWeek =@DayOfWeek and Period=@Period And TeacherID=@TeacherId";
            int occupied = 0;
            using (SqlConnection conn = new SqlConnection(connectionString)) { 
                SqlCommand cmd = new SqlCommand(query,conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@DayOfWeek", day);
                cmd.Parameters.AddWithValue("@Period", period);
                cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                occupied=Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
            }
            if (occupied >= 1) { 
                return false;
            }
            return true;
           
        }
        private void SaveTimetableToDatabase(List<TimeTable> timetable)
        {
            string query = "INSERT INTO tblTimetable (ClassID, SubjectID, TeacherID, Period, DayOfWeek) VALUES (@ClassID, @SubjectID, @TeacherID, @PeriodID, @DayOfWeek)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                foreach (var slot in timetable)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ClassID", slot.ClassID);
                    if (slot.SubjectID.Value ==- 1 )
                    {
                        cmd.Parameters.AddWithValue("@SubjectID", DBNull.Value);
                        
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@SubjectID", slot.SubjectID.Value);
                    }

                    // Ensure TeacherID is passed as a nullable int or DBNull
                    if (slot.TeacherID.Value==-1)
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", slot.TeacherID.Value); 
                    }
                    //cmd.Parameters.AddWithValue("@SubjectID", slot.SubjectID);
                    //cmd.Parameters.AddWithValue("@TeacherID", slot.TeacherID);
                    cmd.Parameters.AddWithValue("@PeriodID", slot.Period);
                    cmd.Parameters.AddWithValue("@DayOfWeek", Convert.ToInt32(slot.DayOfWeek));

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        protected void btnViewTimeTable_Click(object sender, EventArgs e)
        {
            string classId = ddlClass.SelectedValue;
            Response.Redirect($"TimeTableView.aspx?classId={classId}");
        }

        protected void btnViewTeacherTimeTable_Click(object sender, EventArgs e)
        {
            Response.Redirect("TeacherTimeTableView.aspx");
        }
    }
}