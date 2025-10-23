using System;
using Objects;

public class UserRepository
{
    private string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool AddUser(User user)
    {
        if (user == null)
            return false;
        try
        {
            using (var conn = new SQLiteConnection(_connectionString)) 
            {
                conn.Open();

                string query = "INSERT INTO Users (first_name, last_name, email, password, role) " +
                               "VALUES (@FirstName, @LastName, @Email, @Password, @Role)";

                using (var cmd = new SQLiteCommand(query, conn)) 
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.first_name);
                    cmd.Parameters.AddWithValue("@LastName", user.last_name);
                    cmd.Parameters.AddWithValue("@Email", user.email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Password", user.password);
                    cmd.Parameters.AddWithValue("@Role", user.role);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }    
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error in AddUser: {ex.Message}");
            return false;
        }
    }

    public User GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Normalize input (avoid case sensitivity or whitespace issues)
        email = email.Trim().ToLower();

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE LOWER(email) = @Email";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                user_id = Convert.ToInt32(reader["user_id"]),
                                first_name = reader["first_name"].ToString(),
                                last_name = reader["last_name"].ToString(),
                                email = reader["email"].ToString(),
                                password = reader["password"].ToString(),
                                role = reader["role"].ToString()
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error in GetUserByEmail: {ex.Message}");
        }

        return null;
    }


    public User GetUserById(int userId)
    {
        if (userId <= 0)
            return null;

        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE user_id = @UserId";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                user_id = Convert.ToInt32(reader["user_id"]),
                                first_name = reader["first_name"].ToString(),
                                last_name = reader["last_name"].ToString(),
                                email = reader["email"].ToString(),
                                password = reader["password"].ToString(),
                                role = reader["role"].ToString()
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error in GetUserById: {ex.Message}");
        }

        return null;
    }

}

