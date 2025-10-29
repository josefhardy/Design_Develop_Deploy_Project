using System;
using System.Collections.Generic;
using System.Data.SQLite;

public class InteractionRepository
{
    private readonly string _connectionString;

    public InteractionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // -----------------------------------------------------------
    // 1️⃣ Record a Supervisor Interaction
    // -----------------------------------------------------------
    public void RecordSupervisorInteraction(int supervisor_id, int student_id, string interaction_type)
    {
        if (supervisor_id <= 0 || student_id <= 0 || string.IsNullOrWhiteSpace(interaction_type))
        {
            Console.WriteLine("Invalid parameters provided to RecordSupervisorInteraction.");
            return;
        }

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string columnToUpdate = interaction_type.ToLower() switch
                {
                    "meeting" => "meetings_booked_last_month",
                    "wellbeing_check" => "wellbeing_checks_last_month",
                    _ => null
                };

                if (columnToUpdate == null)
                {
                    Console.WriteLine("Invalid interaction type.");
                    return;
                }

                string updateQuery = $@"
                    UPDATE Supervisors
                    SET {columnToUpdate} = {columnToUpdate} + 1
                    WHERE supervisor_id = @SupervisorId";

                using (var cmd = new SQLiteCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@SupervisorId", supervisor_id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recording supervisor interaction: {ex.Message}");
        }
    }

    // -----------------------------------------------------------
    // 2️⃣ Get Supervisor Activity (meetings + wellbeing checks)
    // -----------------------------------------------------------
    public (int meetingsBooked, int wellbeingChecks) GetSupervisorActivity(int supervisor_id)
    {
        if (supervisor_id <= 0)
        {
            Console.WriteLine("Invalid supervisor ID provided to GetSupervisorActivity.");
            return (0, 0);
        }

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT meetings_booked_last_month, wellbeing_checks_last_month
                    FROM Supervisors
                    WHERE supervisor_id = @SupervisorId";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SupervisorId", supervisor_id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int meetings = Convert.ToInt32(reader["meetings_booked_last_month"]);
                            int checks = Convert.ToInt32(reader["wellbeing_checks_last_month"]);
                            return (meetings, checks);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting supervisor activity: {ex.Message}");
        }

        return (0, 0);
    }

    // -----------------------------------------------------------
    // 3️⃣ Get All Meetings (Student Interactions)
    // -----------------------------------------------------------
    public List<Meeting> GetStudentInteractions(int student_id)
    {
        var meetings = new List<Meeting>();

        if (student_id <= 0)
        {
            Console.WriteLine("Invalid student ID in GetStudentInteractions.");
            return meetings;
        }

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT meeting_id, student_id, supervisor_id, meeting_date, start_time, end_time, notes
                    FROM Meetings
                    WHERE student_id = @StudentId
                    ORDER BY meeting_date DESC";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", student_id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            meetings.Add(MapReaderToMeeting(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching student interactions: {ex.Message}");
        }

        return meetings;
    }

    // -----------------------------------------------------------
    // 4️⃣ Get Student Interaction Count
    // -----------------------------------------------------------
    public int GetStudentInteractionCount(int student_id)
    {
        if (student_id <= 0)
        {
            Console.WriteLine("Invalid student ID in GetStudentInteractionCount.");
            return 0;
        }

        try
        {
            int meetingCount = 0;
            int wellbeingChecks = 0;

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                // Count meetings
                string meetingQuery = "SELECT COUNT(*) FROM Meetings WHERE student_id = @StudentId";
                using (var cmd = new SQLiteCommand(meetingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", student_id);
                    meetingCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Get supervisor wellbeing checks for the student
                string wellbeingQuery = @"
                    SELECT s.wellbeing_checks_last_month
                    FROM Students st
                    JOIN Supervisors s ON st.supervisor_id = s.supervisor_id
                    WHERE st.student_id = @StudentId";

                using (var cmd = new SQLiteCommand(wellbeingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", student_id);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        wellbeingChecks = Convert.ToInt32(result);
                }
            }

            return meetingCount + wellbeingChecks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting student interaction count: {ex.Message}");
            return 0;
        }
    }

    // -----------------------------------------------------------
    // 5️⃣ Get All Supervisor Interactions
    // -----------------------------------------------------------
    public List<(Supervisor supervisor, int totalInteractions)> GetAllSupervisorInteractions()
    {
        var results = new List<(Supervisor, int)>();

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT s.supervisor_id, s.meetings_booked_last_month, s.wellbeing_checks_last_month,
                           u.user_id, u.first_name, u.last_name, u.email, u.role
                    FROM Supervisors s
                    JOIN Users u ON s.user_id = u.user_id";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var supervisor = new Supervisor
                            {
                                supervisor_id = Convert.ToInt32(reader["supervisor_id"]),
                                user_id = Convert.ToInt32(reader["user_id"]),
                                first_name = reader["first_name"].ToString(),
                                last_name = reader["last_name"].ToString(),
                                email = reader["email"].ToString(),
                                role = reader["role"].ToString(),
                                meetings_booked_last_month = Convert.ToInt32(reader["meetings_booked_last_month"]),
                                wellbeing_checks_last_month = Convert.ToInt32(reader["wellbeing_checks_last_month"])
                            };

                            int total = supervisor.meetings_booked_last_month + supervisor.wellbeing_checks_last_month;
                            results.Add((supervisor, total));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all supervisor interactions: {ex.Message}");
        }

        return results;
    }

    // -----------------------------------------------------------
    // 6️⃣ Get All Student Interactions (summary list)
    // -----------------------------------------------------------
    public List<(Student student, int totalInteractions)> GetAllStudentInteractions()
    {
        var results = new List<(Student, int)>();

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT st.student_id, st.supervisor_id, st.wellbeing_score, st.last_status_update,
                           u.user_id, u.first_name, u.last_name, u.email, u.password, u.role,
                           COUNT(m.meeting_id) AS meeting_count
                    FROM Students st
                    JOIN Users u ON st.user_id = u.user_id
                    LEFT JOIN Meetings m ON st.student_id = m.student_id
                    GROUP BY st.student_id";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var student = new Student
                            {
                                student_id = Convert.ToInt32(reader["student_id"]),
                                user_id = Convert.ToInt32(reader["user_id"]),
                                supervisor_id = Convert.ToInt32(reader["supervisor_id"]),
                                first_name = reader["first_name"].ToString(),
                                last_name = reader["last_name"].ToString(),
                                email = reader["email"].ToString(),
                                wellbeing_score = Convert.ToInt32(reader["wellbeing_score"]),
                                last_status_update = reader["last_status_update"] == DBNull.Value
                                                     ? (DateTime?)null
                                                     : Convert.ToDateTime(reader["last_status_update"]),
                                role = reader["role"].ToString()
                            };

                            int totalInteractions = Convert.ToInt32(reader["meeting_count"]);
                            results.Add((student, totalInteractions));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all student interactions: {ex.Message}");
        }

        return results;
    }

    // -----------------------------------------------------------
    // Helper Mapper
    // -----------------------------------------------------------
    private Meeting MapReaderToMeeting(SQLiteDataReader reader)
    {
        return new Meeting
        {
            meeting_id = Convert.ToInt32(reader["meeting_id"]),
            student_id = Convert.ToInt32(reader["student_id"]),
            supervisor_id = Convert.ToInt32(reader["supervisor_id"]),
            meeting_date = Convert.ToDateTime(reader["meeting_date"]),
            start_time = Convert.ToDateTime(reader["start_time"]),
            end_time = Convert.ToDateTime(reader["end_time"]),
            notes = reader["notes"].ToString()
        };
    }
}
