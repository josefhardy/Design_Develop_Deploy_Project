using System;
using Design_Develop_Deploy_Project.Objects;

public class Student: User
{
    public int student_id {  get; set; }
    public int supervisor_id { get; set; }
    public int wellbeing_score { get; set; }
    public DateTime? last_status_update { get; set; }

	public Student(int Student_id, int Supervisor_id, int Wellbeing_score, DateTime? Last_status_update, string first_name, string last_name, string email, string password, string role): base(first_name, last_name, email, password, role)
	{
        student_id = Student_id;
        supervisor_id = Supervisor_id;
        wellbeing_score = Wellbeing_score;
        last_status_update = Last_status_update;
	}

    public Student() { }
}





