using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TimeTableGenerator.Data;

namespace TimeTableGenerator
{
    public partial class TeacherTimeTableGenerator : System.Web.UI.Page
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBCS"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateTeacherDropdown();
            }
        }

        private void PopulateTeacherDropdown()
        {
            string query = "SELECT TeacherID, TeacherName FROM tblTeacherList";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlTeacher.DataSource = reader;
                ddlTeacher.DataTextField = "TeacherName";
                ddlTeacher.DataValueField = "TeacherID";
                ddlTeacher.DataBind();
            }
            PopulateClassDropDown();
        }

        private void PopulateClassDropDown()
        {
            string query = "SELECT ClassID, ClassLevelID, ClassName + '-' + Section AS DisplayClass FROM tblClassList";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                cklClass.DataSource = reader;
                cklClass.DataTextField = "DisplayClass";
                cklClass.DataValueField = "ClassLevelID";
                cklClass.DataBind();
            }
        }

        private void PopulateSubjectsForClass(string className, string classlevelID, List<string> storedStates)
        {
            string query = "SELECT CAST(cls.SubjectID AS VARCHAR) + '-' + CAST(cls.ClassLevelID AS VARCHAR) AS ClassSubjectID, SubjectName FROM tblClassLevelSubject AS cls JOIN tblSubjectList AS s ON cls.SubjectID = s.SubjectID JOIN tblClassLevel AS c ON cls.ClassLevelID = c.ClassLevelID WHERE cls.ClassLevelID = @ClassLevelID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClassLevelID", classlevelID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                Label lblClassHeader = new Label();
                lblClassHeader.Text = "<b>Subjects for Class: " + className + "</b><br />";
                subjectsContainer.Controls.Add(lblClassHeader);

                Panel classPanel = new Panel();

                while (reader.Read())
                {
                    string subjectID = reader["ClassSubjectID"].ToString();
                    CheckBox ckbSubject = new CheckBox
                    {
                        ID = "ckbSubject_" + subjectID,
                        Text = reader["SubjectName"].ToString(),
                        Checked = storedStates.Contains("ckbSubject_" + subjectID)
                    };

                    TextBox txtInput = new TextBox
                    {
                        ID = "txt_" + subjectID,
                        CssClass = "subject-input",
                        Width = Unit.Pixel(100),
                    };
                    txtInput.Attributes["placeholder"] = "Enter no. periods";

                    Panel rowPanel = new Panel();
                    rowPanel.Attributes["data-classname"] = className;
                    rowPanel.Controls.Add(ckbSubject);
                    rowPanel.Controls.Add(new Literal { Text = "&nbsp;&nbsp;" });
                    rowPanel.Controls.Add(txtInput);
                    rowPanel.Controls.Add(new Literal { Text = "<br />" });

                    classPanel.Controls.Add(rowPanel);
                }

                subjectsContainer.Controls.Add(classPanel);
                subjectsContainer.Controls.Add(new Literal { Text = "<hr />" });
            }
        }

        protected void btnSubmitClass_Click(object sender, EventArgs e)
        {
            subjectsContainer.Controls.Clear();
            List<string> checkboxStates = new List<string>();

            foreach (ListItem item in cklClass.Items)
            {
                if (item.Selected)
                {
                    string classlevelID = item.Value;
                    string className = item.Text;
                    PopulateSubjectsForClass(className, classlevelID, new List<string>());
                }
            }

            foreach (Control control in subjectsContainer.Controls)
            {
                if (control is Panel classPanel)
                {
                    string className = "";
                    if (subjectsContainer.Controls[subjectsContainer.Controls.IndexOf(control) - 1] is Label lbl)
                    {
                        className = lbl.Text.Replace("<b>Subjects for Class: ", "").Replace("</b><br />", "");
                    }

                    foreach (Control child in classPanel.Controls)
                    {
                        if (child is CheckBox cb && cb.Checked)
                        {
                            string txtID = "txt_" + cb.ID.Replace("ckbSubject_", "");
                            TextBox txt = classPanel.FindControl(txtID) as TextBox;
                            string periods = txt != null ? txt.Text.Trim() : "0";
                            checkboxStates.Add($"{cb.ID}::{className}::{cb.Text}::{periods}");
                        }
                    }
                }
            }

            hfCheckboxStates.Value = string.Join(",", checkboxStates);
        }

        private List<(string ClassName, string SubjectName, int PeriodsPerWeek)> GetSelectedSubjects()
        {
            var selectedSubjects = new List<(string ClassName, string SubjectName, int PeriodsPerWeek)>();
                if (string.IsNullOrEmpty(hfCheckboxStates.Value)) return selectedSubjects;

            var entries = hfCheckboxStates.Value.Split(',');
            foreach (var entry in entries)
            {
                var parts = entry.Split(new[] { "::" }, StringSplitOptions.None);
                if (parts.Length == 4)
                {
                    string className = parts[1];
                    string subjectName = parts[2];
                    int.TryParse(parts[3], out int periods);
                    selectedSubjects.Add((className, subjectName, periods));
                }
            }
            return selectedSubjects;
        }

        protected void btnGenerateTimetable_Click(object sender, EventArgs e)
        {
            var selectedSubjects = GetSelectedSubjects();

            if (selectedSubjects.Count == 0)
            {
                timetableContainer.Text = "<span style='color:red;'>Please select at least one subject.</span>";
                return;
            }

            if (!int.TryParse(txtPeriodsPerDay.Text, out int periodsPerDay) || periodsPerDay <= 0)
            {
                timetableContainer.Text = "<span style='color:red;'>Enter a valid number of periods per day.</span>";
                return;
            }

            if (!int.TryParse(txtPeriodsPerWeek.Text, out int totalPeriodsPerWeek) || totalPeriodsPerWeek <= 0)
            {
                timetableContainer.Text = "<span style='color:red;'>Enter a valid number of periods per week.</span>";
                return;
            }

            GenerateAndDisplayTimetable(selectedSubjects, periodsPerDay);
        }

        private void GenerateAndDisplayTimetable(List<(string ClassName, string SubjectName, int PeriodsPerWeek)> assignments, int maxPeriodsPerDay)
        {
            string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            int totalDays = days.Length;
            int totalPeriods = totalDays * maxPeriodsPerDay;

            Random random = new Random();
            List<TimeSlot> timetable = new List<TimeSlot>();

            // Track how many periods are assigned per day
            Dictionary<string, int> dailyPeriodCount = new Dictionary<string, int>();
            foreach (var day in days) dailyPeriodCount[day] = 0;

            // Track subjects scheduled on each day to avoid duplicates
            Dictionary<string, HashSet<string>> dailySubjects = new Dictionary<string, HashSet<string>>();
            foreach (var day in days) dailySubjects[day] = new HashSet<string>();

            // Assign periods for each subject
            foreach (var (className, subjectName, periodsRequired) in assignments)
            {
                int assigned = 0;

                while (assigned < periodsRequired)
                {
                    // Try 100 times to find a valid random slot
                    bool scheduled = false;
                    for (int attempts = 0; attempts < 100 && !scheduled; attempts++)
                    {
                        string day = days[random.Next(totalDays)];
                        int period = random.Next(1, maxPeriodsPerDay + 1);

                        // Check: max periods on that day not exceeded AND subject not already on that day
                        if (dailyPeriodCount[day] < maxPeriodsPerDay &&
                            !dailySubjects[day].Contains($"{className}-{subjectName}") &&
                            !timetable.Any(t => t.Day == day && t.Period == period))
                        {
                            timetable.Add(new TimeSlot
                            {
                                Day = day,
                                Period = period,
                                ClassName = className,
                                SubjectName = subjectName
                            });

                            dailyPeriodCount[day]++;
                            dailySubjects[day].Add($"{className}-{subjectName}");
                            assigned++;
                            scheduled = true;
                        }
                    }

                    // If unable to assign a slot after many attempts, break to prevent infinite loop
                    if (!scheduled)
                    {
                        break;
                    }
                }
            }

            // Display timetable as HTML table
            System.Text.StringBuilder html = new System.Text.StringBuilder();
            html.Append("<table border='1' cellpadding='8'><tr><th>Day</th><th>Period</th><th>Class</th><th>Subject</th></tr>");

            foreach (var slot in timetable.OrderBy(t => Array.IndexOf(days, t.Day)).ThenBy(t => t.Period))
            {
                html.Append("<tr>");
                html.Append($"<td>{slot.Day}</td>");
                html.Append($"<td>{slot.Period}</td>");
                html.Append($"<td>{slot.ClassName}</td>");
                html.Append($"<td>{slot.SubjectName}</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            timetableContainer.Text = html.ToString();
        }



        private void SaveTimetableToDatabase(int teacherId, List<TimeSlot> timetable)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (var slot in timetable)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO tblTimetable (ClassID, SubjectID, TeacherID, Period, DayOfWeek) VALUES (@ClassID, @SubjectID, @TeacherID, @PeriodID, @Day)", conn);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherId);
                    cmd.Parameters.AddWithValue("@Day", slot.Day);
                    cmd.Parameters.AddWithValue("@Period", slot.Period);
                    cmd.Parameters.AddWithValue("@ClassName", slot.ClassName);
                    cmd.Parameters.AddWithValue("@SubjectName", slot.SubjectName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected void ddlTeacher_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateClassDropDown();
        }
    }

    public class TimeSlot
    {
        public string Day { get; set; }
        public int Period { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
    }
}
