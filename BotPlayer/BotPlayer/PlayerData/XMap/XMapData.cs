using System.Collections.Generic;
using System.Linq;
using BotPlayer.Classes;

namespace BotPlayer.PlayerData.XMap;

public class XMapData(Player player)
{
    public bool IsLoading;
    public Dictionary<int, List<MapNext>> MyLinkMaps;
    public Player Player = player;

    private void LoadLinkMapBase()
    {
        MyLinkMaps = new Dictionary<int, List<MapNext>>();
        LoadLinkMapsAutoWaypointFromFile();
        LoadLinkMapsHome();
    }

    public void Update()
    {
        LoadLinkMapBase();
        IsLoading = false;
    }

    public void LoadLinkMaps()
    {
        IsLoading = true;
    }

    public List<MapNext> GetMapNexts(int idMap)
    {
        return CanGetMapNexts(idMap) ? MyLinkMaps[idMap] : null;
    }

    private void LoadLinkMapsAutoWaypointFromFile()
    {
        foreach (var data in Const.LinkMap)
            for (var i = 0; i < data.Length; i++)
            {
                if (i != 0)
                    LoadLinkMap(data[i], data[i - 1], null);

                if (i != data.Length - 1)
                    LoadLinkMap(data[i], data[i + 1], null);
            }
    }

    private void LoadLinkMapsHome()
    {
        var idMapHome = 21 + Player.Gender;
        var idMapLang = 7 * Player.Gender;
        LoadLinkMap(idMapLang, idMapHome, null);
        LoadLinkMap(idMapHome, idMapLang, null);
    }

    public bool CanGetMapNexts(int idMap)
    {
        return MyLinkMaps.ContainsKey(idMap);
    }

    private void LoadLinkMap(int idMapStart, int idMapNext, int[] info)
    {
        AddKeyLinkMaps(idMapStart);
        MapNext mapNext = new(idMapNext, info);
        MyLinkMaps[idMapStart].Add(mapNext);
    }

    private void AddKeyLinkMaps(int idMap)
    {
        if (!MyLinkMaps.ContainsKey(idMap))
            MyLinkMaps.Add(idMap, []);
    }

    public static Waypoint FindWaypoint(Map map, int idMap)
    {
        return map.Waypoints.FirstOrDefault(t => t.Name.Equals(Const.MapNames[idMap]));
    }

    public static int GetXInsideMap(Map map, Waypoint waypoint)
    {
        return waypoint.MaxX < 24 ? 24 :
            waypoint.MinX > map.Tmw * 24 - 24 ? map.Tmw * 24 - 24 :
            waypoint.MinX + (waypoint.MaxX - waypoint.MinX) / 2;
    }

    public static int GetPosWaypointX(Map map, Waypoint waypoint)
    {
        if (waypoint.MaxX < 60)
            return 15;
        if (waypoint.MinX > map.Tmw * 24 - 60)
            return map.Tmw * 24 - 15;
        return waypoint.MinX + 30;
    }

    public static int GetPosWaypointY(Waypoint waypoint)
    {
        return waypoint.MaxY;
    }

    public void Dispose()
    {
        Player = null;
        MyLinkMaps?.Clear();
        MyLinkMaps = null;
    }
}