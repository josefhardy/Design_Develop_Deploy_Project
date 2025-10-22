using System;
using System.Collections.Generic;
using Design_Develop_Deploy_Project.Objects;

public class SeniorTutor : User
{
    public int seniorTutor_id { get; set; }
    public List<int> supervisor_ids { get; set; }

    public SeniorTutor(int seniorTutorId, string firstName, string lastName, string email, string password, string role,List<int> supervisorIds = null) : base(firstName, lastName, email, password, role)
    {
        seniorTutor_id = seniorTutorId;
        supervisor_ids = supervisorIds ?? new List<int>();
    }

    public SeniorTutor()
    {
        supervisor_ids = new List<int>();
    }
}
