using System;

public class SupervisorRepository
{
	public string _connectionString;

	public SupervisorRepository(string connectionString)
	{
		_connectionString = connectionString;
	}

	public Supervisor GetSupervisorById(int Supervisor_id) 
	{
		if(Supervisor_id <= 0) { return null;}

		using (var conn = new SQLiteConnection(_connectionString)) 
		{
			conn.Open();
			string query = @"SELECT s.supervisor_id,
							u.user_id, u.first_name, u.last_name, u.email, u.password, u.role
							FROM Supervisors s
							JOIN Users u on s.user_id =  u.user_id
							WHERE s.supervisor_id = @Supervisor_id";

			using (var cmd = new SQLiteCommand(query, conn)) 
			{
				cmd.Parameters.AddWithValue("@Supervisor_id", Supervisor_id);
				using (var reader = cmd.ExecuteReader()) 
				{
					if (reader.Read()) 
					{
						return MapReaderToSupervisor(reader);
					}
					else 
					{
						return null;
                    }
                }
            }
        }
		return null;
    }

	public Supervisor GetSupervisorByEmail(string Email) 
	{
		if(string.IsNullOrWhiteSpace(Email)) { return null;}
		using (var conn = new SQLiteConnection(_connectionString)) 
		{
			conn.Open();
			string query = @"SELECT s.supervisor_id,
							u.user_id, u.first_name, u.last_name, u.email, u.password, u.role
							FROM Supervisors s
							JOIN Users u on s.user_id =  u.user_id
							WHERE LOWER(u.email) = @Email";
			using (var cmd = new SQLiteCommand(query, conn)) 
			{
				cmd.Parameters.AddWithValue("@Email", Email.Trim().ToLower());
				using (var reader = cmd.ExecuteReader()) 
				{
					if (reader.Read()) 
					{
						return MapReaderToSupervisor(reader);
					}
				}
            }
			return null;
        }
    }

	public void RecordSupervisorInteraction(int supervisor_id, int student_id, string interaction_type) 
	{
		
	}

	public int GetSupervisorActivity() 
	{
		
	}

    private Supervisor MapReaderToSupervisor(SQLiteDataReader reader)
    {
        return new Supervisor
        {
            user_id = Convert.ToInt32(reader["user_id"]),
            supervisor_id = Convert.ToInt32(reader["supervisor_id"]),
            first_name = reader["first_name"].ToString(),
            last_name = reader["last_name"].ToString(),
            email = reader["email"].ToString(),
            password = reader["password"].ToString(),
            role = reader["role"].ToString(),
        };
    }


