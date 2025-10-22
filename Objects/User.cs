using System;

public class User
{
	public int user_id { get; set; }
	public int supervisor_id { get; set; }
	public string first_name { get; set; }
	public string last_name { get; set; }
	public string email { get; set; }
	public string password { get; set; }
	public string role { get; set; }

	public User(string Firstname, string Lastname, string Email, string Password, string Role)
    {
		first_name= Firstname;
		last_name= Lastname;
		email = Email;
		password = Password;
		role = Role;
    }
	public User() { }
}
