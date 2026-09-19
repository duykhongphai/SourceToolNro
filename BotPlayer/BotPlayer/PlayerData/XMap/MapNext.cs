namespace BotPlayer.PlayerData.XMap;

public struct MapNext(int mapId, int[] info)
{
    public readonly int MapId = mapId;

    public int[] Info = info;
}