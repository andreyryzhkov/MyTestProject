namespace MyTestProject;

public class Player
{
    
    //private int Id { get; set; }
    
    //private string Name { get; set; }
    private int _id;
    
    public int Id
    {
        get => _id;
        set => _id = value;
    }

    private string _name;

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public Player()
    {
        
    }

    public Player(int id, string name)
    {
        Id = id;
        Name = name;
    }
}