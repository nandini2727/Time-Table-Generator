<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TeacherTimeTableGenerator.aspx.cs" Inherits="TimeTableGenerator.TeacherTimeTableGenerator" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Timetable Generator</h2>

            <!-- Select Class Dropdown -->
            <div>
                <label for="ddlTeacher">Select Teacher:</label>
                <asp:DropDownList ID="ddlTeacher" runat="server" Width="200px" OnSelectedIndexChanged="ddlTeacher_SelectedIndexChanged" AutoPostBack="true">
                </asp:DropDownList>
            </div>
            <br />
            <div id="classContainer" runat="server">
                <label for="ddlClass">Select Class:</label>
                <asp:CheckBoxList ID="cklClass" runat="server" Width="200px" ></asp:CheckBoxList>
                <asp:Button ID="btnSubmitClass" runat="server" Text="Submit" OnClick ="btnSubmitClass_Click" />
                <%--<asp:DropDownList ID="ddlClass" runat="server" Width="200px" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" AutoPostBack="true">
                </asp:DropDownList>--%>
            </div>
            <!-- List of Subjects with Input Fields for Periods -->
            <div id="subjectsContainer" runat="server">
    
            </div>

            <br />
            <asp:Label ID="lblPeriodsPerDay" runat="server" Text="Periods per Day:"></asp:Label>
            <asp:TextBox ID="txtPeriodsPerDay" runat="server"></asp:TextBox>
            <br />

            <asp:Label ID="lblPeriodsPerWeek" runat="server" Text="Periods per Week:"></asp:Label>
            <asp:TextBox ID="txtPeriodsPerWeek" runat="server"></asp:TextBox>
            <br />

            <asp:Button ID="btnGenerateTimetable" runat="server" Text="Generate Timetable" OnClick="btnGenerateTimetable_Click" />
            <br /><br />

            <asp:Literal ID="timetableContainer" runat="server"></asp:Literal>
           <asp:HiddenField ID="hfCheckboxStates" runat="server" />
        </div>
    </form>
    <script>
        function updateCheckboxStates() {
            var selected = [];
            var checkboxes = document.querySelectorAll("input[type='checkbox']");

            checkboxes.forEach(cb => {
                if (cb.checked) {
                    var subjectId = cb.id; // e.g., ckbSubject_1-2
                    var subjectName = cb.nextSibling.textContent.trim(); // assumes label is inline
                    var parent = cb.parentNode; // or use cb.parentNode if not using <div>s
                    var classLabel = "";

                    // Find the nearest previous label for class name
                    var sibling = cb;
                    while (sibling.previousSibling) {
                        sibling = sibling.previousSibling;
                        if (sibling.tagName === "LABEL") {
                            classLabel = sibling.textContent.replace("Subjects for Class:", "").trim();
                            break;
                        }
                    }

                    // Find the corresponding textbox for periods
                    var txtBox = parent.querySelector("input[type='text']");
                    var periodCount = txtBox ? txtBox.value.trim() : "0";

                    // Format: checkboxID::ClassName::SubjectName::Periods
                    selected.push(`${subjectId}::${classLabel}::${subjectName}::${periodCount}`);
                }
            });

            document.getElementById('<%= hfCheckboxStates.ClientID %>').value = selected.join(',');
        }
        window.onload = function () {
            document.querySelectorAll("input[type='checkbox']").forEach(cb => {
                cb.addEventListener("change", updateCheckboxStates);
            });

            document.querySelectorAll("input[type='text']").forEach(txt => {
                txt.addEventListener("input", updateCheckboxStates); // update when typing
            });
        };

    </script>
</body>
</html>
