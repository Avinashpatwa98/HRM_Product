/*namespace Attendance_maintaince.Views
{
    document.addEventListener("DOMContentLoaded", function () {
        const loginForm = document.getElementById("loginForm");
        const addEmployeeForm = document.getElementById("addEmployeeForm");
        const addAttendanceForm = document.getElementById("addAttendanceForm");

        // Example of form validation or submission
        if (loginForm) {
            loginForm.onsubmit = function (event) {
                const email = loginForm.email.value;
                const password = loginForm.password.value;

                if (!email || !password) {
                    alert("Email and Password are required!");
                    event.preventDefault();
                }
            };
        }

        if (addEmployeeForm) {
            addEmployeeForm.onsubmit = function (event) {
                const name = addEmployeeForm.name.value;
                const email = addEmployeeForm.email.value;
                const position = addEmployeeForm.position.value;

                if (!name || !email || !position) {
                    alert("All fields are required!");
                    event.preventDefault();
                }
            };
        }

        if (addAttendanceForm) {
            addAttendanceForm.onsubmit = function (event) {
                const isPresent = addAttendanceForm.isPresent.checked;

                if (!isPresent) {
                    alert("Please indicate attendance status!");
                    event.preventDefault();
                }
            };
        }
    });


}
*/