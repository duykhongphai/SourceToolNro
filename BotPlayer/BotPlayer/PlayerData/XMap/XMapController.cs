using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotPlayer.Classes;
using BotPlayer.Client;

namespace BotPlayer.PlayerData.XMap;

public class XMapController(Player player)
{
    private const int TimeDelayNextmap = 200;
    private const int TimeDelayRenextmap = 500;
    private int _idMapEnd;
    private int _indexWay;
    private bool _isChangingMap;
    private bool _isNextMapFailed;
    private bool _isWait;
    private bool _isWaitNextMap;
    private Player _player = player;
    private long _timeStartWait;
    private long _timeWait;
    private List<int> _wayXmap;

    public void Dispose()
    {
        _wayXmap?.Clear();
        _player = null;
        _wayXmap = null;
    }

    public async void Update()
    {
        if (IsWaiting())
            return;
        if (_isWaitNextMap)
        {
            Wait(TimeDelayNextmap);
            _isWaitNextMap = false;
            return;
        }

        if (_isNextMapFailed)
        {
            _player.XMapData.MyLinkMaps = null;
            _wayXmap = null;
            _isNextMapFailed = false;
            return;
        }

        if (_wayXmap == null)
        {
            if (_player.XMapData.MyLinkMaps == null)
            {
                _player.XMapData.LoadLinkMaps();
                return;
            }

            _wayXmap = XMapAlgorithm.FindWay(_player, _player.Map.MapId, _idMapEnd);
            _indexWay = 0;
            if (_wayXmap == null)
            {
                FinishXmap();
                return;
            }
        }

        if (_player.Map.MapId == _wayXmap[^1] && _player.Hp > 0)
        {
            FinishXmap();
            return;
        }

        if (_player.Map.MapId == _wayXmap[_indexWay])
        {
            if (_player.Hp <= 0)
            {
                _isWaitNextMap = _isNextMapFailed = true;
            }
            else
            {
                await NextMap(_wayXmap[_indexWay + 1]);
                _isWaitNextMap = true;
            }

            Wait(TimeDelayRenextmap);
            return;
        }

        if (_player.Map.MapId == _wayXmap[_indexWay + 1])
        {
            _indexWay++;
            return;
        }

        _isNextMapFailed = true;
    }

    public void FinishXmap()
    {
        _player.XMapMain.IsXMapRunning = false;
        _isNextMapFailed = false;
        _player.XMapData.MyLinkMaps = null;
        _wayXmap = null;
    }

    private void Wait(int time)
    {
        _isWait = true;
        _timeStartWait = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _timeWait = time;
    }

    private bool IsWaiting()
    {
        if (_isWait && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _timeStartWait >= _timeWait)
            _isWait = false;
        return _isWait;
    }

    private async Task NextMap(int idMapNext)
    {
        var mapNexts = _player.XMapData.GetMapNexts(_player.Map.MapId);
        if (mapNexts == null) return;
        foreach (var mapNext in mapNexts.Where(mapNext => mapNext.MapId == idMapNext))
        {
            await NextMap(mapNext);
            return;
        }
    }

    private async Task NextMap(MapNext mapNext)
    {
        await NextMapAutoWaypoint(mapNext);
    }

    public void StartRunToMapId(int idMap)
    {
        _idMapEnd = idMap;
        _player.XMapMain.IsXMapRunning = true;
    }

    private async Task NextMapAutoWaypoint(MapNext mapNext)
    {
        var waypoint = XMapData.FindWaypoint(_player.Map, mapNext.MapId);
        if (waypoint == null) return;
        if (_isChangingMap)
            return;
        _isChangingMap = true;
        await Task.Run(async () =>
        {
            try
            {
                const int size = 24;
                var start = new Tile(_player.X / size, _player.Y / size - 1);
                var destination = new Tile(XMapData.GetXInsideMap(_player.Map, waypoint) / size,
                    waypoint.MinY / size);
                var path = XMapAStar.FindPath(_player.Map, start, destination);
                if (path.Count == 0)
                {
                    FinishXmap();
                    _isChangingMap = false;
                    return;
                }

                var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var mapId = _player.Map.MapId;
                while (path.Count > 0)
                {
                    if (_player.Map.MapId != mapId)
                        break;
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > 60000)
                    {
                        ChangeMap(waypoint);
                        _isChangingMap = false;
                        return;
                    }

                    var tile = path.Pop();
                    var sleep = 0;
                    var xEnd = tile.X * size;
                    var yEnd = (tile.Y + 1) * size;
                    _player.CurrentMovePoint = new MovePoint(xEnd, yEnd);
                    while (Function.GetDistance(_player.X, _player.Y, xEnd, yEnd) > size * 2)
                    {
                        if (player.Map.MapId != mapId)
                            break;
                        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > 60000)
                        {
                            ChangeMap(waypoint);
                            _isChangingMap = false;
                            return;
                        }

                        if (sleep % 500 == 0)
                        {
                            if (sleep >= 1500)
                            {
                                int mutation;
                                do
                                {
                                    mutation = Function.NextInt(-2, 3);
                                } while (mutation == 0);

                                xEnd = tile.X * size + mutation * size / 2;
                                do
                                {
                                    mutation = Function.NextInt(-2, 3);
                                } while (mutation == 0);

                                yEnd = (tile.Y + 1) * size + mutation * size / 2;
                            }

                            if (sleep >= 5000)
                            {
                                start = new Tile(_player.X / size, _player.Y / size - 1);
                                path = XMapAStar.FindPath(_player.Map, start, destination);
                                if (path.Count == 0)
                                {
                                    FinishXmap();
                                    _isChangingMap = false;
                                    return;
                                }

                                break;
                            }

                            if (_player.CurrentMovePoint == null ||
                                _player.CurrentMovePoint.XEnd != xEnd ||
                                _player.CurrentMovePoint.YEnd != tile.Y * size + yEnd)
                                _player.CurrentMovePoint = new MovePoint(xEnd, yEnd);
                        }

                        await Task.Delay(100);
                        sleep += 100;
                    }
                }

                if (Function.GetDistance(_player.X, _player.Y, XMapData.GetXInsideMap(_player.Map, waypoint),
                        waypoint.MinY) <=
                    size * 2)
                    RequestChangeMap(waypoint);
                await Task.Delay(500);
            }
            catch
            {
            }
            finally
            {
                _isChangingMap = false;
            }
        });
    }

    private void ChangeMap(Waypoint waypoint)
    {
        if (waypoint == null) return;
        TeleportMyChar(XMapData.GetPosWaypointX(_player.Map, waypoint), XMapData.GetPosWaypointY(waypoint));
        RequestChangeMap(waypoint);
    }

    private void TeleportMyChar(int x, int y)
    {
        _player.CurrentMovePoint = null;
        Service.Instance.CharMove(_player.Session, (short)x, (short)y);
        Service.Instance.CharMove(_player.Session, (short)x, (short)(y + 1));
        Service.Instance.CharMove(_player.Session, (short)x, (short)y);
    }

    private void RequestChangeMap(Waypoint waypoint)
    {
        if (waypoint.IsOffline)
        {
            Service.Instance.GetMapOffline(_player.Session);
            return;
        }

        Service.Instance.RequestChangeMap(_player.Session);
    }
}