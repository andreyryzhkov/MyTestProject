using System.Collections;

namespace MyTestProject;

public interface IPlayerDao
{
    public ArrayList getPlayers();

    public void setPlayer(Player player);
}