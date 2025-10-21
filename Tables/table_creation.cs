using System;
using System.Data.SQLite;
using System.IO;

string dbpath = "Project_database.db";

if (!File.Exists(dbpath)) 
{
    SQLiteConnection.CreateFile(dbpath);
    Console.WriteLine("Database created");
}

string conn_string = $"Data Source = {dbpath};Version=3";

using (var conn = new SQLiteConnection(conn_string)) 
{
    conn.Open();

    using (var pragmaCommand = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn))
    {
        pragmaCommand.ExecuteNonQuery();
    }

    string Create_all_tables = @"
        CREATE TABLE IF NOT EXISTS Users (
            user_id INTEGER PRIMARY KEY AUTOINCREMENT,
            first_name TEXT NOT NULL,
            last_name TEXT NOT NULL, 
            email TEXT NOT NULL UNIQUE, 
            password TEXT NOT NULL, 
            role TEXT NOT NULL CHECK(role in ('student', 'supervisor', 'senior_tutor'))
            );

       CREATE TABLE IF NOT EXISTS Supervisors(
            supervisor_id INTEGER PRIMARY KEY AUTOINCREMENT, 
            user_id INTEGER NOT NULL, 
            FOREIGN KEY (user_id) REFERENCES Users (user_id)
        );

       CREATE TABLE IF NOT EXISTS Students (
            student_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL, 
            supervisor_id INTEGER NOT NULL,
            wellbeing_score INTEGER DEFAULT 5, 
            last_status_update TEXT, 
            FOREIGN KEY (user_id) REFERENCES Users(user_id),
            FOREIGN KEY (supervisor_id) REFERENCES Supervisors (supervisor_id)
            );

       CREATE TABLE IF NOT EXISTS Meetings (
            meeting_id INTEGER PRIMARY KEY AUTOINCREMENT, 
            student_id INTEGER NOT NULL, 
            supervisor_id INTEGER NOT NULL, 
            meeting_date TEXT NOT NULL, 
            notes TEXT, 
            FOREIGN KEY (student_id) REFERENCES Students(student_id),
            FOREIGN KEY (supervisor_id) REFERENCES Supervisors(supervisor_id)
            );
        
       CREATE TABLE IF NOT EXISTS Senior_Tutors(
            senior_tutor_id INTEGER PRIMARY KEY AUTOINCREMENT, 
            user_id INTEGER NOT NULL, 
            FOREIGN KEY (user_id) REFERENCES Users(user_id)
            );
";

    

    using (var command = new SQLiteCommand(Create_all_tables, conn)) { command.ExecuteNonQuery(); }
    
}