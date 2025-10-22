using System;
using System.Collections.Generic;
using Design_Develop_Deploy_Project.Objects;

public class Supervisor : User 
{
    public int supervisor_id { get; set; }
    public List<int> student_ids { get; set; }

    //having a populated and empty constructor for flexibility when creating objects in different scenarious 
	public Supervisor(int Supervisor_id, List<int> Student_ids, string first_name, string last_name, string email,string password, string role) : base(first_name, last_name, email, password, role)
    {
		supervisor_id = Supervisor_id;
		student_ids = Student_ids;
    }

	public Supervisor() { student_ids = new List<int>(); }
} 
    

