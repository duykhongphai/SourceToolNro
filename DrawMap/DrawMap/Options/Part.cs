namespace DrawMap.Options;

public class Part(int type)
{
    public PartImage[] Pi = type switch
    {
        0 => new PartImage[3],
        1 => new PartImage[17],
        2 => new PartImage[14],
        3 => new PartImage[2],
        _ => null
    };

    public void Dispose()
    {
        Pi = null;
    }
}