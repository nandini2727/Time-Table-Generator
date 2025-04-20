<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TimeTableGenerator.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Timetable Generator</title>
    <link href="Styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Timetable Generator</h2>

            <!-- Select Class Dropdown -->
            <div>
                <label for="ddlClass">Select Class:</label>
                <asp:DropDownList ID="ddlClass" runat="server" Width="200px" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" AutoPostBack="true">
                </asp:DropDownList>
            </div>
            <br />

            <!-- List of Subjects with Input Fields for Periods -->
            <div id="subjectsContainer" runat="server">
                
            </div>

            <br />
            <!-- Save Button -->
            <div>
                <asp:Button ID="btnGenerateTimetable" runat="server" Text="Generate Timetable" OnClick="btnGenerateTimetable_Click" />
                <asp:Button ID="btnViewTimeTable" runat="server" Text="View Timetable" OnClick="btnViewTimeTable_Click" />
                <asp:Button ID="btnViewTeacherTimeTable" runat="server" Text="View Teacher Timetable" OnClick="btnViewTeacherTimeTable_Click" />
            </div>
        </div>
    </form>
</body>
</html>
