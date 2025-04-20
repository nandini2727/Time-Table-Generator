using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TimeTableGenerator.Data;

namespace TimeTableGenerator
{
	public partial class TeacherTimeTableView : System.Web.UI.Page
	{
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBCS"].ToString();
        protected void Page_Load(object sender, EventArgs e)
		{
            if (!IsPostBack) { 
                
                BindDropDown();
                BindGridView();
            }
            
        }
        private void BindDropDown() {
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
            ddlTeachers.DataSource= teachers;
            ddlTeachers.DataTextField = "TeacherName";
            ddlTeachers.DataValueField = "TeacherID";
            ddlTeachers.DataBind();
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            BindGridView();
        }
        private void BindGridView()
        {
            string teacherId = ddlTeachers.SelectedValue;
            
            string query = $"select c.ClassID,c.SubjectId,c.TeacherID,c.DayOfWeek,Period, SubjectName,l.TeacherName,cl.ClassName from tblTimeTable as c join tblSubjectList as  t on c.SubjectID = t.SubjectID join tblTeacherList as l on c.TeacherID =l.TeacherID join tblClassList as cl on c.ClassID=cl.ClassID  where c.TeacherID={teacherId} ORDER BY Period, DayOfWeek";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                conn.Open();

                da.Fill(dt);
            }
            // Step 2: Create a DataTable for the GridView
            DataTable timetable = new DataTable();
            timetable.Columns.Add("Days"); // To hold the days (Monday to Friday)
            timetable.Columns.Add("I");
            timetable.Columns.Add("II");
            timetable.Columns.Add("III");
            timetable.Columns.Add("IV");
            timetable.Columns.Add("Lunch");
            timetable.Columns.Add("V");
            timetable.Columns.Add("VI");
            timetable.Columns.Add("VII");
            timetable.Columns.Add("VIII");

            // Step 3: Populate the timetable DataTable

            for (int dayOfWeek = 1; dayOfWeek <= 6; dayOfWeek++) // Loop through days (1=Monday, 5=Friday)
            {
                DataRow newRow = timetable.NewRow();

                // Set the day name (Monday, Tuesday, etc.)
                switch (dayOfWeek)
                {
                    case 1: newRow["Days"] = "Monday"; break;
                    case 2: newRow["Days"] = "Tuesday"; break;
                    case 3: newRow["Days"] = "Wednesday"; break;
                    case 4: newRow["Days"] = "Thursday"; break;
                    case 5: newRow["Days"] = "Friday"; break;
                    case 6: newRow["Days"] = "Saturday"; break;
                }

                // Fill in the periods for this day (1 to 8)
                for (int period = 1, i = 1; period <= 8; i++) // Loop through periods (I to VIII)
                {
                    var matchingRow = dt.AsEnumerable()
                        .FirstOrDefault(row => row.Field<int>("Period") == period && row.Field<int>("DayOfWeek") == dayOfWeek);

                    if (matchingRow != null)
                    {
                        if (i == 5) // The Lunch period (assuming it's the 5th period)
                        {
                            newRow["Lunch"] = "";

                        }
                        else
                        {
                            newRow[i] = matchingRow["SubjectName"] + " - " + matchingRow["ClassName"];
                            period++;
                        }
                    }
                    else
                    {
                        if (period == 5) // No subject, leave "Lunch" as "Lunch Break"
                        {
                            newRow["Lunch"] = "";
                            period++;
                        }
                        else
                        {
                            newRow[period] = ""; // No subject for this period on this day
                            period++;
                        }
                    }
                }

                timetable.Rows.Add(newRow);
            }
            // Step 4: Bind the timetable DataTable to the GridView
            GridView1.DataSource = timetable;
            GridView1.DataBind();
        }

        
    }
}