namespace BotPlayer.PlayerData;

public class ItemMap
{
    public int ItemMapId;
    public int ItemTemplateId;
    public int PlayerId;
    public int X;
    public int Y;

    public ItemMap(int playerId, short itemMapId, short itemTemplateId, int x, int y, short r)
    {
        ItemMapId = itemMapId;
        ItemTemplateId = itemTemplateId;
        X = x;
        Y = y;
        PlayerId = playerId;
    }
}