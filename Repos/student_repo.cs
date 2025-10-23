using System;
using Design.Develop.Deploy.Objects;

public class StudentRepository
{
	private string _connectionString;

	public StudentRepository(string connectionString)
	{
		_connectionString = connectionString;
	}

	public bool AddStudent(Student student)
	{
		if (student == null) return false;

		using (var conn = new SQLiteConnection(_connectionString))
		{
			conn.Open();
			using (var transaction = conn.BeginTransaction())
			{
				string userQuery = @"
                INSERT INTO Users (first_name, last_name, email, password, role)
                VALUES (@FirstName, @LastName, @Email, @Password, @Role);
                SELECT last_insert_rowid();";

				long userId;
				using (var cmd = new SQLiteCommand(userQuery, conn, transaction))
				{
					cmd.Parameters.AddWithValue("@FirstName", student.first_name);
					cmd.Parameters.AddWithValue("@LastName", student.last_name);
					cmd.Parameters.AddWithValue("@Email", student.email.Trim().ToLower());
					cmd.Parameters.AddWithValue("@Password", student.password);
					cmd.Parameters.AddWithValue("@Role", "student");
					userId = (long)cmd.ExecuteScalar();
				}

				string studentQuery = @"
                INSERT INTO Students (user_id, supervisor_id, wellbeing_score, last_status_update)
                VALUES (@UserId, @SupervisorId, @WellbeingScore, @LastStatusUpdate)";

				using (var cmd = new SQLiteCommand(studentQuery, conn, transaction))
				{
					cmd.Parameters.AddWithValue("@UserId", userId);
					cmd.Parameters.AddWithValue("@SupervisorId", student.supervisor_id);
					cmd.Parameters.AddWithValue("@WellbeingScore", student.wellbeing_score);
					cmd.Parameters.AddWithValue("@LastStatusUpdate", student.last_status_update ?? DateTime.UtcNow);
					cmd.ExecuteNonQuery();
				}

				transaction.Commit();
				return true;
			}
		}
	}


	public void DeleteStudent(int studentId)
	{
		try
		{
			using (var conn = new SQLiteConnection(_connectionString))
			{
				conn.Open();
				string query = "DELETE FROM Students WHERE student_id = @StudentId";
				using (var cmd = new SQLiteCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@StudentId", studentId);
					cmd.ExecuteNonQuery();
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Database error in DeleteStudent: {ex.Message}");
		}
	}

	public Student GetStudentByEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			return null;

		using (var conn = new SQLiteConnection(_connectionString))
		{
			conn.Open();

			string query = @"
			SELECT s.student_id, s.supervisor_id, s.wellbeing_score, s.last_status_update,
				   u.user_id, u.first_name, u.last_name, u.email, u.password, u.role
			FROM Students s
			JOIN Users u ON s.user_id = u.user_id
			WHERE LOWER(u.email) = @Email";


			using (var cmd = new SQLiteCommand(query, conn))
			{
				cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return MapReaderToStudent(reader);
                    }
					else
					{
						return null;
					}
				}
			}
		}

	}

	public Student GetStudentById(int studentId)
	{
		if (studentId <= 0)
		{
			return null;
		}
		using (var conn = new SQLiteConnection(_connectionString))
		{
			conn.Open();
			string query = @"
			SELECT s.student_id, s.supervisor_id, s.wellbeing_score, s.last_status_update,
				   u.user_id, u.first_name, u.last_name, u.email, u.password, u.role
			FROM Students s
			JOIN Users u ON s.user_id = u.user_id
			WHERE s.student_id = @StudentId";


			using (var cmd = new SQLiteCommand(query, conn))
			{
				cmd.Parameters.AddWithValue("@StudentId", studentId);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return MapReaderToStudent(reader);

                    }
					else
					{
						return null;
					}
				}
			}
		}
	}

	public bool UpdateStudentWellbeing(int studentId, int wellbeingScore)
	{
		try
		{
			using (var conn = new SQLiteConnection(_connectionString))
			{
				conn.Open();
				string query = "UPDATE Students SET wellbeing_score = @WellbeingScore, last_status_update = @LastStatusUpdate " +
							   "WHERE student_id = @StudentId";
				using (var cmd = new SQLiteCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@WellbeingScore", wellbeingScore);
					cmd.Parameters.AddWithValue("@LastStatusUpdate", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@StudentId", studentId);

					int rowsaffected = cmd.ExecuteNonQuery();
					return rowsaffected > 0;
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Database error in UpdateStudentWellbeing: {ex.Message}");
			return false;
		}
	}

	public List<Student> GetAllStudentsUnderSpecificSupervisor(int supervisorId)
	{
		var students = new List<Student>();
		try
		{
			using (var conn = new SQLiteConnection(_connectionString))
			{
				conn.Open();
				string query = @"
				SELECT s.student_id, s.supervisor_id, s.wellbeing_score, s.last_status_update,
					   u.user_id, u.first_name, u.last_name, u.email, u.password, u.role
				FROM Students s
				JOIN Users u ON s.user_id = u.user_id
				WHERE s.supervisor_id = @SupervisorId";
				using (var cmd = new SQLiteCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@SupervisorId", supervisorId);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var student = MapReaderToStudent(reader);
                            students.Add(student);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Database error in GetAllStudentsUnderSpecificSupervisor: {ex.Message}");
		}
		return students;
	}

	public List<Student> GetAllStudentsByWellBeingScore(int minScore, int maxScore)
	{
		var students = new List<Student>();
		try
		{
			using (var conn = new SQLiteConnection(_connectionString))
			{
				conn.Open();
				string query = @"
				SELECT s.student_id, s.supervisor_id, s.wellbeing_score, s.last_status_update,
					   u.user_id, u.first_name, u.last_name, u.email, u.password, u.role
				FROM Students s
				JOIN Users u ON s.user_id = u.user_id
				WHERE s.wellbeing_score BETWEEN @MinScore AND @MaxScore";
				using (var cmd = new SQLiteCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@MinScore", minScore);
					cmd.Parameters.AddWithValue("@MaxScore", maxScore);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var student = MapReaderToStudent(reader);
                            students.Add(student);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Database error in GetAllStudentsByWellBeingScore: {ex.Message}");
		}
		return students;
	}

    private Student MapReaderToStudent(SQLiteDataReader reader)
    {
        return new Student
        {
            user_id = Convert.ToInt32(reader["user_id"]),
            student_id = Convert.ToInt32(reader["student_id"]),
            first_name = reader["first_name"].ToString(),
            last_name = reader["last_name"].ToString(),
            email = reader["email"].ToString(),
            password = reader["password"].ToString(),
            supervisor_id = Convert.ToInt32(reader["supervisor_id"]),
            wellbeing_score = Convert.ToInt32(reader["wellbeing_score"]),
            last_status_update = reader["last_status_update"] == DBNull.Value
                                 ? (DateTime?)null
                                 : Convert.ToDateTime(reader["last_status_update"]),
            role = reader["role"].ToString()
        };
    }


