using System.Collections;
using Npgsql;

namespace MyTestProject;

public class PlayerDao : IPlayerDao
{
    private static string SELECT_PLAYERS = @"select id, name from player";
    private static string INSERT_PLAYER = @"INSERT INTO player (id, name) VALUES (@id, @name)";
    public void setPlayer(Player player)
    {
        using (var conn = PgConnection.GetConnection())
        {
            Console.Out.WriteLine("Opening connection");
            conn.Open();
            
            using (NpgsqlCommand cmd = new NpgsqlCommand(INSERT_PLAYER, conn))
            {
                cmd.Parameters.AddWithValue("@id", player.Id);
                cmd.Parameters.AddWithValue("@name", player.Name);
               
                try
                {
                   cmd.ExecuteNonQuery();
                }
                catch (NpgsqlException ex)
                { 
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            conn.Close();
        }

    }
    public ArrayList getPlayers()
    {
        ArrayList players = new ArrayList();

        using (var conn = PgConnection.GetConnection())
        {
            Console.Out.WriteLine("Opening connection");
            conn.Open();

            using (var command = new NpgsqlCommand(SELECT_PLAYERS, conn))
            {
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Player player = new Player();
                    
                    player.Id = reader.GetInt32(0);
                    player.Name = reader.GetString(1);

                    players.Add(player);
                }
                
            }

            conn.Close();
        }

        return players;
    }
}