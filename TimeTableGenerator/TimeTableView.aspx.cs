using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TimeTableGenerator
{
    public partial class TimeTableView : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) {
                BindGridView();
            }
        }
        private void BindGridView()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBCS"].ToString();
            int classId = 1;
            if (Request.QueryString != null)
            {
                classId = Convert.ToInt32(Request.QueryString["classId"]);
            }

            string query = $"select c.ClassID,c.SubjectId,c.TeacherID,c.DayOfWeek,Period, SubjectName,l.TeacherName from tblTimeTable as c join tblSubjectList as  t on c.SubjectID = t.SubjectID join tblTeacherList as l on c.TeacherID =l.TeacherID where ClassID={classId} ORDER BY Period, DayOfWeek";
            
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
                for (int period = 1,i=1; period <= 8; i++) // Loop through periods (I to VIII)
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
                            newRow[i] = matchingRow["SubjectName"] + " - " + matchingRow["TeacherName"];
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