<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TeacherTimeTableView.aspx.cs" Inherits="TimeTableGenerator.TeacherTimeTableView" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Teacher Timetable</h2>
            <asp:DropDownList ID="ddlTeachers" runat="server" ></asp:DropDownList>
            <asp:Button ID="btnSubmit" runat="server" Text="OK" OnClick="btnSubmit_Click" />
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="Days" HeaderText="Days" SortExpression="Days" />
                    <asp:BoundField DataField="I" HeaderText="I" SortExpression="I" />
                    <asp:BoundField DataField="II" HeaderText="II" SortExpression="II" />
                    <asp:BoundField DataField="III" HeaderText="III" SortExpression="III" />
                    <asp:BoundField DataField="IV" HeaderText="IV" SortExpression="IV" />
                    <asp:BoundField DataField="Lunch" HeaderText="Lunch" SortExpression="Lunch" />
                    <asp:BoundField DataField="V" HeaderText="V" SortExpression="V" />
                    <asp:BoundField DataField="VI" HeaderText="VI" SortExpression="VI" />
                    <asp:BoundField DataField="VII" HeaderText="VII" SortExpression="VII" />
                    <asp:BoundField DataField="VIII" HeaderText="VIII" SortExpression="VIII" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
