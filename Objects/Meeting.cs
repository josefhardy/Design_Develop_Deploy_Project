using System;

public class Meeting
{
    public int meeting_id { get; set; }
    public int student_id { get; set; }
    public int supervisor_id { get; set; }
    public DateTime meeting_date { get; set; }       
    public TimeSpan start_time { get; set; }         
    public TimeSpan end_time { get; set; }           
    public string notes { get; set; }
    public DateTime created_at { get; set; }         
    public DateTime updated_at { get; set; }         

    public Meeting() { }

    public Meeting(int meetingId,int studentId,int supervisorId,DateTime meetingDate,TimeSpan startTime,TimeSpan endTime,string notes,DateTime createdAt,DateTime updatedAt)
    {
        meeting_id = meetingId;
        student_id = studentId;
        supervisor_id = supervisorId;
        meeting_date = meetingDate;
        start_time = startTime;
        end_time = endTime;
        this.notes = notes;
        created_at = createdAt;
        updated_at = updatedAt;
    }
}

