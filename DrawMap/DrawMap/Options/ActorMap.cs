namespace DrawMap.Options;

public class ActorMap(int id, int x, int y, byte typeActor)
{
    public int Id = id;
    public byte TypeActor = typeActor;
    public int X = x;
    public int Y = y;
}