﻿using Microsoft.Data.SqlClient;
using Roommates.Models;
using System.Collections.Generic;

namespace Roommates.Repositories
{
    /// <summary>
    ///  This class is responsible for interacting with Room data.
    ///  It inherits from the BaseRepository class so that it can use the BaseRepository's Connection property
    /// </summary>
    public class RoomRepository : BaseRepository
    {
        /// <summary>
        ///  When new RoomRepository is instantiated, pass the connection string along to the BaseRepository
        /// </summary>
        public RoomRepository(string connectionString) : base(connectionString) { }

        // ...We'll add some methods shortly...

        /// <summary>
        ///  Get a list of all Rooms in the database
        /// </summary>
        public List<Room> GetAll()
        {
            //  We must "use" the database connection.
            //  Because a database is a shared resource (other applications may be using it too) we must
            //  be careful about how we interact with it. Specifically, we Open() connections when we need to
            //  interact with the database and we Close() them when we're finished.
            //  In C#, a "using" block ensures we correctly disconnect from a resource even if there is an error.
            //  For database connections, this means the connection will be properly closed.
            using (SqlConnection conn = Connection)
            {
                // Note, we must Open() the connection, the "using" block doesn't do that for us.
                conn.Open();

                // We must "use" commands too.
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    cmd.CommandText = "SELECT Id, Name, MaxOccupancy FROM Room";

                    // Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();

                    // A list to hold the rooms we retrieve from the database.
                    List<Room> rooms = new List<Room>();

                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        // The "ordinal" is the numeric position of the column in the query results.
                        //  For our query, "Id" has an ordinal value of 0 and "Name" is 1.
                        int idColumnPosition = reader.GetOrdinal("Id");

                        // We user the reader's GetXXX methods to get the value for a particular ordinal.
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColumnPosition = reader.GetOrdinal("Name");
                        string nameValue = reader.GetString(nameColumnPosition);

                        int maxOccupancyColumPosition = reader.GetOrdinal("MaxOccupancy");
                        int maxOccupancy = reader.GetInt32(maxOccupancyColumPosition);

                        // Now let's create a new room object using the data from the database.
                        Room room = new Room
                        {
                            Id = idValue,
                            Name = nameValue,
                            MaxOccupancy = maxOccupancy,
                        };

                        // ...and add that room object to our list.
                        rooms.Add(room);
                    }

                    // We should Close() the reader. Unfortunately, a "using" block won't work here.
                    reader.Close();

                    // Return the list of rooms who whomever called this method.
                    return rooms;
                }
            }
        }

        public Room GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Name, MaxOccupancy From Room Where Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    Room room = null;

                    if (reader.Read())
                    {            
                        room = new Room
                        {
                            Id = id,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            MaxOccupancy = reader.GetInt32(reader.GetOrdinal("MaxOccupancy")),
                        };
                    }

                    reader.Close();

                    return room;
                }
            }
        }
        
        public void Insert(Room room)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Room (Name, MaxOccupancy)
                                                OUTPUT INSERTED.Id
                                                VALUES (@name, @maxOccupancy)";
                    cmd.Parameters.AddWithValue("@name", room.Name);
                    cmd.Parameters.AddWithValue("@maxOccupancy", room.MaxOccupancy);
                    int id = (int)cmd.ExecuteScalar();

                    room.Id = id;
                }
            }
        }

       
        ///  Updates the room

        public void Update(Room room)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Room
                                    SET Name = @name,
                                        MaxOccupancy = @maxOccupancy
                                    WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@name", room.Name);
                    cmd.Parameters.AddWithValue("@maxOccupancy", room.MaxOccupancy);
                    cmd.Parameters.AddWithValue("@id", room.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
