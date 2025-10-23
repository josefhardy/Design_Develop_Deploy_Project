using System;

public class Meeting
{
    public int meeting_id { get; set; }
    public int student_id { get; set; }
    public int supervisor_id { get; set; }
    public DateTime meeting_date { get; set; }
    public string notes { get; set; }

    public Meeting(int meetingId, int studentId, int supervisorId, DateTime meetingDate, string notes)
    {
        meeting_id = meetingId;
        student_id = studentId;
        supervisor_id = supervisorId;
        meeting_date = meetingDate;
        this.notes = notes;
    }
    public Meeting() { }
}
