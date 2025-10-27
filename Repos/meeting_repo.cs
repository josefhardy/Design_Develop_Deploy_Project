using System;

public class MeeitngRepository
{
	public string _connectionString { get; set; }

    public Class1(string connectionString)
	{
		_connectionString = connectionString;
    }

	public class MeetingRepository() { }

    public bool AddMeeting(Meeting meeting)
    {
        if (meeting == null) throw new ArgumentNullException(nameof(meeting));
        if (meeting.student_id <= 0 || meeting.supervisor_id <= 0)
            throw new ArgumentException("Invalid student or supervisor ID.");
        if (meeting.start_time >= meeting.end_time)
            throw new ArgumentException("Start time must be before end time.");
        if (meeting.meeting_date.Date < DateTime.Today)
            throw new ArgumentException("Cannot schedule a meeting in the past.");

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                // 1. Check for conflicts
                string conflictQuery = @"
                SELECT COUNT(*) 
                FROM Meetings
                WHERE supervisor_id = @SupervisorId
                  AND meeting_date = @MeetingDate
                  AND ((@StartTime < end_time AND @EndTime > start_time))";

                using (var cmd = new SQLiteCommand(conflictQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@SupervisorId", meeting.supervisor_id);
                    cmd.Parameters.AddWithValue("@MeetingDate", meeting.meeting_date.Date);
                    cmd.Parameters.AddWithValue("@StartTime", meeting.start_time);
                    cmd.Parameters.AddWithValue("@EndTime", meeting.end_time);

                    int conflictCount = Convert.ToInt32(cmd.ExecuteScalar());
                    if (conflictCount > 0)
                    {
                        // Conflict exists
                        return false;
                    }
                }

                // 2. Insert the meeting
                string insertQuery = @"
                INSERT INTO Meetings (student_id, supervisor_id, meeting_date, start_time, end_time, notes)
                VALUES (@StudentId, @SupervisorId, @MeetingDate, @StartTime, @EndTime, @Notes)";

                using (var cmd = new SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", meeting.student_id);
                    cmd.Parameters.AddWithValue("@SupervisorId", meeting.supervisor_id);
                    cmd.Parameters.AddWithValue("@MeetingDate", meeting.meeting_date.Date);
                    cmd.Parameters.AddWithValue("@StartTime", meeting.start_time);
                    cmd.Parameters.AddWithValue("@EndTime", meeting.end_time);
                    cmd.Parameters.AddWithValue("@Notes", meeting.notes ?? "");

                    cmd.ExecuteNonQuery();
                }

                // Optionally, update supervisor's meetings booked count
                string updateSupervisorQuery = @"
                UPDATE Supervisors
                SET meetings_booked_last_month = meetings_booked_last_month + 1
                WHERE supervisor_id = @SupervisorId";

                using (var cmd = new SQLiteCommand(updateSupervisorQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@SupervisorId", meeting.supervisor_id);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding meeting: {ex.Message}");
            return false;
        }
    }


    public Meeting GetMeeting(int meetingId) 
	{
		
    }

	public List<Meeting> GetUsersMeeting(int userId) 
	{

    }

	public void UpdateMeeting(Meeting meeting)
	{

	}

	public void DeleteMeeting(int meetingId) 
	{

    }

    public List<TimeSlot> FetchAvailableSlots(int supervisorId, DateTime desiredDate)
    {
        if (desiredDate.Date < DateTime.Today)
            throw new ArgumentException("Cannot fetch slots for past dates.");
        if (supervisorId <= 0)
            throw new ArgumentException("Invalid supervisor ID.");

        var availableSlots = new List<TimeSlot>();
        var takenSlots = new List<TimeSlot>();

        using (var conn = new SQLiteConnection(_connectionString))
        {
            conn.Open();

            // 1. Get meetings already booked for this supervisor on the desired date
            string meetingsQuery = @"
            SELECT start_time, end_time 
            FROM Meetings
            WHERE supervisor_id = @SupervisorId
              AND meeting_date = @MeetingDate";
            using (var cmd = new SQLiteCommand(meetingsQuery, conn))
            {
                cmd.Parameters.AddWithValue("@SupervisorId", supervisorId);
                cmd.Parameters.AddWithValue("@MeetingDate", desiredDate.Date);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime start = DateTime.Parse(reader["start_time"].ToString());
                        DateTime end = DateTime.Parse(reader["end_time"].ToString());
                        takenSlots.Add(new TimeSlot(start, end));
                    }
                }
            }

            // 2. Get office hours for the supervisor
            string officeHoursQuery = @"
            SELECT office_hours 
            FROM Supervisors
            WHERE supervisor_id = @SupervisorId";
            string officeHoursString;
            using (var cmd = new SQLiteCommand(officeHoursQuery, conn))
            {
                cmd.Parameters.AddWithValue("@SupervisorId", supervisorId);
                officeHoursString = cmd.ExecuteScalar()?.ToString();
            }

            if (string.IsNullOrWhiteSpace(officeHoursString))
                return new List<TimeSlot>();

            var officeHourRanges = officeHoursString.Split(',')
                .Select(slot =>
                {
                    var times = slot.Split('-');
                    DateTime start = DateTime.ParseExact(times[0], "HH:mm", null);
                    DateTime end = DateTime.ParseExact(times[1], "HH:mm", null);
                    
                    start = new DateTime(desiredDate.Year, desiredDate.Month, desiredDate.Day, start.Hour, start.Minute, 0);
                    end = new DateTime(desiredDate.Year, desiredDate.Month, desiredDate.Day, end.Hour, end.Minute, 0);
                    return new TimeSlot(start, end);
                }).ToList();

            foreach (var officeSlot in officeHourRanges)
            {
                DateTime currentStart = officeSlot.Start;

                foreach (var booked in takenSlots.Where(b => b.Start < officeSlot.End && b.End > officeSlot.Start)
                                                .OrderBy(b => b.Start))
                {
                    if (currentStart < booked.Start)
                        availableSlots.Add(new TimeSlot(currentStart, booked.Start));

                    currentStart = booked.End > currentStart ? booked.End : currentStart;
                }

                if (currentStart < officeSlot.End)
                    availableSlots.Add(new TimeSlot(currentStart, officeSlot.End));
            }
        }

        return availableSlots;
    }


}
