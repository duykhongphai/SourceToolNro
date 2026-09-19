using System;
using System.Linq;
using BotPlayer.Classes;
using BotPlayer.Client;
using BotPlayer.PlayerData.XMap;

namespace BotPlayer.PlayerData;

public class Player
{
    private short _gameTick;
    private long _lastTimeChangeZone;
    private int _mapGoBack = -1;
    private int _randomXGo;
    public int CharId;
    public MovePoint CurrentMovePoint;
    public sbyte Flag;
    public sbyte Gender;
    public int Hp;
    public int HpFull;
    public Map Map = new();
    public Mob MobTarget;
    public int Mp;
    public int MpFull;
    public Player PlayerTarget;
    public Session Session;
    public sbyte Speed;
    public int X;
    public XMapController XMapController;
    public XMapData XMapData;
    public XMapMain XMapMain;
    public int Y;

    public Player(Session ss)
    {
        Session = ss;
        XMapData = new XMapData(this);
        XMapMain = new XMapMain(this);
        XMapController = new XMapController(this);
    }

    public void Update()
    {
        try
        {
            XMapMain.Update();
            _gameTick++;
            if (_gameTick > 10000) _gameTick = 0;
            if (Map.MapId == Gender + 21 && _gameTick % 20 == 0)
            {
                foreach (var itemMap in Map.ItemMaps.Where(itemMap =>
                             (itemMap.PlayerId == CharId || itemMap.PlayerId == -1) &&
                             itemMap.ItemTemplateId == 74))
                    Service.Instance.PickItem(Session, itemMap.ItemMapId);
                Service.Instance.OpenMenu(Session, 4);
                Service.Instance.Menu(Session, 4, 0, 0);
            }

            if ((Hp <= HpFull * 20 / 100 || Mp <= MpFull * 20 / 100) && _gameTick % 20 == 0)
                Service.Instance.UseItem(Session, 0, 1, -1, 13);
            if (_mapGoBack != -1 && Map.MapId == _mapGoBack && _gameTick % 20 == 0)
            {
                if (_randomXGo == -1) _randomXGo = Function.NextInt(2, Map.Tmw) * 24;
                CurrentMovePoint = new MovePoint(_randomXGo, Y);
            }

            if (!XMapMain.IsXMapRunning)
            {
                if ((Map.MapId == Gender + 21 || Map.MapId == 41 || Map.MapId == 39 || Map.MapId == 40) &&
                    _gameTick % 20 == 0)
                {
                    var waypoint = Map.Waypoints[0];
                    if (!Function.IsInWayPoint(this, waypoint))
                    {
                        CurrentMovePoint = new MovePoint(waypoint.MaxX - 20, waypoint.MaxY);
                        return;
                    }

                    CurrentMovePoint = null;
                    Service.Instance.GetMapOffline(Session);
                }

                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTimeChangeZone > 30000)
                {
                    var zoneId = Map.FindZoneLowPlayer();
                    if (zoneId != -1 && zoneId != -2)
                        Service.Instance.RequestChangeZone(Session, zoneId);
                    else
                        AutoChangeRandomMap();
                    _lastTimeChangeZone = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
        }
        catch
        {
        }

        if (CurrentMovePoint == null || Session == null) return;
        if (Math.Abs(X - CurrentMovePoint.XEnd) <= 16 && Math.Abs(Y - CurrentMovePoint.YEnd) <= 16)
        {
            X = CurrentMovePoint.XEnd;
            Y = CurrentMovePoint.YEnd;
        }
        else
        {
            var xDirection = Math.Sign(CurrentMovePoint.XEnd - X);
            var yDirection = Math.Sign(CurrentMovePoint.YEnd - Y);
            X += xDirection * Speed * 3;
            if ((xDirection == 1 && X > CurrentMovePoint.XEnd) || (xDirection == -1 && X < CurrentMovePoint.XEnd))
                X = CurrentMovePoint.XEnd;
            Y += yDirection * Speed;
            if ((yDirection == 1 && Y > CurrentMovePoint.YEnd) || (yDirection == -1 && Y < CurrentMovePoint.YEnd))
                Y = CurrentMovePoint.YEnd;
        }

        Service.Instance.CharMove(Session, (short)X, (short)Y);
    }

    public void AutoChangeRandomMap()
    {
        var listMap = Const.ListIdMap[Gender];
        _mapGoBack = listMap[Function.NextInt(0, listMap.Length - 1)];
        _randomXGo = -1;
        XMapController.StartRunToMapId(_mapGoBack);
    }

    public void AutoPlay()
    {
        if (XMapMain.IsXMapRunning) return;
        var mob = ClosestMob();
        if (mob == null) return;
        MobTarget = mob;
        CurrentMovePoint = new MovePoint(mob.x, mob.y);
        if (Function.GetDistance(mob, this) > 50) return;
        Service.Instance.SelectSkill(Session, Gender switch
        {
            0 => 0,
            1 => 2,
            _ => 4
        });
        Service.Instance.SendPlayerAttack(Session, mob, null);
    }

    private Mob ClosestMob()
    {
        if (MobTarget is { hp: > 0 }) return MobTarget;
        Mob result = null;
        var minDistance = int.MaxValue;
        foreach (var mob in Map.Mobs)
        {
            if (mob.hp <= 0) continue;
            var distance = Function.GetDistance(mob, this);
            if (minDistance <= distance) continue;
            minDistance = distance;
            result = mob;
        }

        return result;
    }

    public void DoPvPNearPlayer()
    {
        if (XMapMain.IsXMapRunning) return;
        if (Flag != 8)
        {
            Service.Instance.ChooseFlag(Session, 8);
            return;
        }

        PlayerTarget ??= Map.FindCharInMapFlag(this);
        if (PlayerTarget == null) return;
        if (PlayerTarget.Flag != 8)
        {
            PlayerTarget = null;
            return;
        }

        if (PlayerTarget.Hp <= 0)
        {
            PlayerTarget = null;
            return;
        }

        CurrentMovePoint = new MovePoint(PlayerTarget.X, PlayerTarget.Y);
        if (Function.GetDistance(this, PlayerTarget) > 30) return;
        Service.Instance.SelectSkill(Session, Gender switch
        {
            0 => 0,
            1 => 2,
            _ => 4
        });
        Service.Instance.SendPlayerAttack(Session, null, PlayerTarget);
    }


    public void Dispose()
    {
        XMapController?.Dispose();
        XMapData?.Dispose();
        Map?.Dispose();
        XMapMain?.Dispose();
        XMapMain = null;
        XMapController = null;
        XMapData = null;
        MobTarget = null;
        Session = null;
        Map = null;
        PlayerTarget = null;
        CurrentMovePoint = null;
    }
}