using System;

public class Status
{
	public int status_id { get; set; }
	public int student_id { get; set; }
	public int score { get; set; }
	public string comments { get; set; }
    public DateTime status_date { get; set; }

    public Status(int Status_id, int Student_id, int Score, string Comments, DateTime Status_Date)
	{
		status_id = Status_id;
		student_id = Student_id;
		score = Score;
		comments = Comments;
		status_date = Status_Date;
	}
	public Status() { }
}
