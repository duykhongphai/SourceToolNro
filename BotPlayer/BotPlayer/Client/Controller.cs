using BotPlayer.PlayerData;

namespace BotPlayer.Client;

public class Controller
{
    public Message messWait;

    public Controller(Session session)
    {
        Session = session;
    }

    public Session Session { get; set; }

    public void OnMessage(Message msg)
    {
        if (!Session.IsConnected()) return;
        try
        {
            switch (msg.Command)
            {
                case -94:

                    break;
                case -82:
                    var b67 = msg.Reader().ReadByte();
                    Session.Player.Map.TileIndex = new int[b67][][];
                    Session.Player.Map.TileType = new int[b67][];
                    for (var num170 = 0; num170 < b67; num170++)
                    {
                        var b68 = msg.Reader().ReadByte();
                        Session.Player.Map.TileType[num170] = new int[b68];
                        Session.Player.Map.TileIndex[num170] = new int[b68][];
                        for (var num171 = 0; num171 < b68; num171++)
                        {
                            Session.Player.Map.TileType[num170][num171] = msg.Reader().ReadInt();
                            var b69 = msg.Reader().ReadByte();
                            Session.Player.Map.TileIndex[num170][num171] = new int[b69];
                            for (var num172 = 0; num172 < b69; num172++)
                                Session.Player.Map.TileIndex[num170][num171][num172] = msg.Reader().ReadByte();
                        }
                    }

                    break;
                case -42:
                    msg.Reader().ReadInt();
                    msg.Reader().ReadInt();
                    msg.Reader().ReadInt();
                    Session.Player.HpFull = msg.Reader().ReadInt();
                    Session.Player.MpFull = msg.Reader().ReadInt();
                    Session.Player.Hp = msg.Reader().ReadInt();
                    Session.Player.Mp = msg.Reader().ReadInt();
                    Session.Player.Speed = msg.Reader().ReadByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadInt();
                    msg.Reader().ReadInt();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadLong();
                    msg.Reader().ReadShort();
                    msg.Reader().ReadShort();
                    msg.Reader().ReadByte();
                    break;
                case -34:
                    break;
                case -68:
                    msg.Reader().ReadShort();
                    break;
                case -28:
                    MessageNotMap(msg);
                    break;
                case -30:
                    MessageSubCommand(msg);
                    break;
                case -21:
                    var itemMapId = msg.Reader().ReadShort();
                    for (var num139 = 0; num139 < Session.Player.Map.ItemMaps.Count; num139++)
                        if (Session.Player.Map.ItemMaps[num139].ItemMapId == itemMapId)
                        {
                            Session.Player.Map.ItemMaps.RemoveAt(num139);
                            break;
                        }

                    break;
                case 68:
                    var itemMapID = msg.Reader().ReadShort();
                    var itemTemplateId = msg.Reader().ReadShort();
                    int x = msg.Reader().ReadShort();
                    int y = msg.Reader().ReadShort();
                    var num114 = msg.Reader().ReadInt();
                    short r = 0;
                    if (num114 == -2) r = msg.Reader().ReadShort();
                    var o2 = new ItemMap(num114, itemMapID, itemTemplateId, x, y, r);
                    Session.Player.Map.ItemMaps.Add(o2);
                    break;
                case 29:
                    var size = msg.Reader().ReadByte();
                    Session.Player.Map.Zones = new int[size];
                    Session.Player.Map.NumPlayer = new int[size];
                    Session.Player.Map.MaxPlayer = new int[size];
                    for (var i = 0; i < size; i++)
                    {
                        Session.Player.Map.Zones[i] = msg.Reader().ReadByte();
                        msg.Reader().ReadByte();
                        Session.Player.Map.NumPlayer[i] = msg.Reader().ReadByte();
                        Session.Player.Map.MaxPlayer[i] = msg.Reader().ReadByte();
                        var b = msg.Reader().ReadByte();
                        if (b != 1) continue;
                        msg.Reader().ReadUTF();
                        msg.Reader().ReadInt();
                        msg.Reader().ReadUTF();
                        msg.Reader().ReadInt();
                    }

                    break;
                case -24:
                    Session.Player.CurrentMovePoint = null;
                    Session.Player.MobTarget = null;
                    Session.Player.PlayerTarget = null;
                    Session.Player.Map.Players.Clear();
                    Session.Player.Map.ItemMaps.Clear();
                    Session.Player.Map.Mobs.Clear();
                    Session.Player.Map.MapId = msg.Reader().ReadUnsignedByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadByte();
                    msg.Reader().ReadUTF();
                    Session.Player.Map.ZoneId = msg.Reader().ReadByte();
                    Service.Instance.RequestMapTemplate(Session, Session.Player.Map.MapId);
                    messWait = msg;
                    break;
                case -25:
                case 94:
                    Session.Player.XMapMain.Info(msg.Reader().ReadUTF());
                    break;
                case -17:
                    Service.Instance.ReturnTownFromDead(Session);
                    break;
                case -7:
                    var charId2 = msg.Reader().ReadInt();
                    for (var num180 = 0; num180 < Session.Player.Map.Players.Count; num180++)
                    {
                        var char14 = Session.Player.Map.Players[num180];
                        if (char14 == null) break;
                        if (char14.CharId != charId2) continue;
                        char14.X = msg.Reader().ReadShort();
                        char14.Y = msg.Reader().ReadShort();
                        break;
                    }

                    break;
                case -74:
                    var type = msg.Reader().ReadSByte();
                    switch (type)
                    {
                        case 0:
                            msg.Reader().ReadInt();
                            break;
                        case 1:
                            msg.Reader().ReadShort();
                            Service.Instance.GetResource(Session, 2);
                            break;
                        case 3:
                            msg.Reader().ReadInt();
                            Service.Instance.GetResource(Session, 3);
                            break;
                    }

                    break;
                case -13:
                    int num189 = msg.Reader().ReadUnsignedByte();
                    if (num189 > Session.Player.Map.Mobs.Count - 1) return;
                    var mob92 = Session.Player.Map.Mobs[num189];
                    mob92.sys = msg.Reader().ReadByte();
                    mob92.levelBoss = msg.Reader().ReadByte();
                    mob92.hp = msg.Reader().ReadInt();
                    mob92.maxHp = mob92.hp;
                    break;
                case -75:
                    Mob mob91 = null;
                    try
                    {
                        mob91 = Session.Player.Map.Mobs[msg.Reader().ReadUnsignedByte()];
                    }
                    catch
                    {
                    }

                    if (mob91 != null) mob91.levelBoss = msg.Reader().ReadByte();
                    break;
                case -9:
                    Mob mob93 = null;
                    try
                    {
                        mob93 = Session.Player.Map.Mobs[msg.Reader().ReadUnsignedByte()];
                    }
                    catch
                    {
                    }

                    if (mob93 != null)
                    {
                        mob93.hp = msg.Reader().ReadInt();
                        msg.Reader().ReadInt();
                        try
                        {
                            msg.Reader().ReadBool();
                        }
                        catch
                        {
                        }

                        msg.Reader().ReadByte();
                    }

                    break;
                case 45:
                    Mob mob94 = null;
                    try
                    {
                        mob94 = Session.Player.Map.Mobs[msg.Reader().ReadUnsignedByte()];
                    }
                    catch
                    {
                    }

                    if (mob94 != null) mob94.hp = msg.Reader().ReadInt();
                    break;
                case -60:
                    var num2 = msg.Reader().ReadInt();
                    var num3 = -1;
                    if (num2 != Session.Player.CharId)
                    {
                        var char2 = Session.Player.Map.FindCharInMap(num2);
                        if (char2 == null) return;

                        msg.Reader().ReadUnsignedByte();
                        var b = msg.Reader().ReadByte();
                        for (var num = 0; num < b; num++) msg.Reader().ReadInt();
                    }
                    else
                    {
                        msg.Reader().ReadByte();
                        msg.Reader().ReadByte();
                        num3 = msg.Reader().ReadInt();
                    }

                    try
                    {
                        var b4 = msg.Reader().ReadByte();
                        if (b4 != 1) break;
                        var b5 = msg.Reader().ReadByte();
                        if (num3 == Session.Player.CharId)
                        {
                            var @char = Session.Player;
                            var num5 = msg.Reader().ReadInt();
                            msg.Reader().ReadBool();
                            msg.Reader().ReadBool();
                            if (b5 == 0) @char.Hp -= num5;
                        }
                        else
                        {
                            var @char = Session.Player.Map.FindCharInMap(num3);
                            if (@char == null) return;
                            var num7 = msg.Reader().ReadInt();
                            msg.Reader().ReadBool();
                            msg.Reader().ReadBool();
                            if (b5 == 0) @char.Hp -= num7;
                        }
                    }
                    catch
                    {
                    }

                    break;
                case -111:
                    break;
                case 2:
                    Session.NeedRegister = true;
                    break;
                case -6:
                    var num177 = msg.Reader().ReadInt();
                    for (var num178 = 0; num178 < Session.Player.Map.Players.Count; num178++)
                    {
                        var char13 = Session.Player.Map.Players[num178];
                        if (char13 == null || char13.CharId != num177) continue;
                        if (Session.Player.PlayerTarget != null && Session.Player.PlayerTarget.CharId == char13.CharId)
                            Session.Player.PlayerTarget = null;
                        Session.Player.Map.Players.RemoveAt(num178);
                        return;
                    }

                    break;
                case -12:

                    Mob mob9 = null;
                    try
                    {
                        mob9 = Session.Player.Map.Mobs[msg.Reader().ReadUnsignedByte()];
                    }
                    catch
                    {
                    }

                    if (mob9 != null)
                    {
                        mob9.hp = 0;
                        try
                        {
                            var num190 = msg.Reader().ReadInt();
                            msg.Reader().ReadBool();
                            var b76 = msg.Reader().ReadByte();
                            for (var num191 = 0; num191 < b76; num191++)
                            {
                                var itemMap4 = new ItemMap(msg.Reader().ReadShort(), msg.Reader().ReadShort(),
                                    (short)mob9.x, (short)mob9.y, msg.Reader().ReadShort(), msg.Reader().ReadShort())
                                {
                                    PlayerId = msg.Reader().ReadInt()
                                };
                                Session.Player.Map.ItemMaps.Add(itemMap4);
                            }
                        }
                        catch
                        {
                        }
                    }

                    break;
                case -103:
                    var b15 = msg.Reader().ReadByte();
                    if (b15 == 1)
                    {
                        var num20 = msg.Reader().ReadInt();
                        var b18 = msg.Reader().ReadByte();
                        if (num20 == Session.Player.CharId)
                            Session.Player.Flag = b18;
                        else if (Session.Player.Map.FindCharInMap(num20) != null)
                            Session.Player.Map.FindCharInMap(num20).Flag = b18;
                    }

                    break;
                case -5:
                    var charId = msg.Reader().ReadInt();
                    msg.Reader().ReadInt();
                    var char15 = new Player(null)
                    {
                        CharId = charId
                    };
                    if (ReadCharInfo(char15, msg))
                    {
                        var b73 = msg.Reader().ReadByte();
                        if (Session.Player.Map.FindCharInMap(char15.CharId) == null)
                        {
                            Service.Instance.ChatPlayer(Session, "Mua Tool Bot Tại XTOOLS247.COM", char15.CharId);
                            Session.Player.Map.Players.Add(char15);
                        }

                        msg.Reader().ReadByte();
                        msg.Reader().ReadShort();
                    }

                    var b74 = msg.Reader().ReadByte();
                    char15.Flag = b74;
                    msg.Reader().ReadByte();
                    try
                    {
                        msg.Reader().ReadShort();
                        msg.Reader().ReadSByte();
                        msg.Reader().ReadShort();
                    }
                    catch
                    {
                    }

                    break;
            }
        }
        catch
        {
        }
    }

    private void MessageNotMap(Message msg)
    {
        var b = msg.Reader().ReadByte();
        switch (b)
        {
            case 4:
                Service.Instance.ClientOk(Session);
                Session.SuccessLogin = true;
                break;
            case 10:
                Session.Player.Map.Maps = null;
                Session.Player.Map.Types = null;
                Session.Player.Map.Tmw = msg.Reader().ReadByte();
                Session.Player.Map.Tmh = msg.Reader().ReadByte();
                Session.Player.Map.Maps = new int[Session.Player.Map.Tmw * Session.Player.Map.Tmh];
                for (var i = 0; i < Session.Player.Map.Maps.Length; i++)
                {
                    int num2 = msg.Reader().ReadByte();
                    if (num2 < 0) num2 += 256;
                    Session.Player.Map.Maps[i] = (ushort)num2;
                }

                Session.Player.Map.Types = new int[Session.Player.Map.Maps.Length];
                msg = messWait;
                LoadInfoMap(msg);
                try
                {
                    msg.Reader().ReadByte();
                }
                catch
                {
                }

                messWait.Cleanup();
                messWait = null;
                break;
        }
    }

    private void MessageSubCommand(Message msg)
    {
        var b = msg.Reader().ReadByte();
        Player @char;
        switch (b)
        {
            case 0:
                Session.Player.CurrentMovePoint = null;
                Session.Player.MobTarget = null;
                Session.Player.PlayerTarget = null;
                Session.Player.Map.Players.Clear();
                Session.Player.Map.ItemMaps.Clear();
                Session.Player.Map.Mobs.Clear();
                Session.Player.CharId = msg.Reader().ReadInt();
                msg.Reader().ReadByte();
                Session.Player.Gender = msg.Reader().ReadByte();
                msg.Reader().ReadShort();
                msg.Reader().ReadUTF();
                msg.Reader().ReadByte();
                msg.Reader().ReadByte();
                msg.Reader().ReadLong();
                msg.Reader().ReadShort();
                msg.Reader().ReadShort();
                msg.Reader().ReadByte();
                var b2 = msg.Reader().ReadByte();
                for (sbyte b6 = 0; b6 < b2; b6++) msg.Reader().ReadShort();
                msg.Reader().ReadLong();
                msg.Reader().ReadInt();
                msg.Reader().ReadInt();
                var sizebody = msg.Reader().ReadByte();
                for (var k = 0; k < sizebody; k++)
                {
                    var num5 = msg.Reader().ReadShort();
                    if (num5 == -1) continue;
                    msg.Reader().ReadInt();
                    msg.Reader().ReadUTF();
                    msg.Reader().ReadUTF();
                    int num7 = msg.Reader().ReadUnsignedByte();
                    if (num7 == 0) continue;
                    for (var l = 0; l < num7; l++)
                    {
                        msg.Reader().ReadUnsignedByte();
                        msg.Reader().ReadUnsignedShort();
                    }
                }

                var sizebag = msg.Reader().ReadByte();
                for (var m = 0; m < sizebag; m++)
                {
                    var num9 = msg.Reader().ReadShort();
                    if (num9 == -1) continue;
                    msg.Reader().ReadInt();
                    msg.Reader().ReadUTF();
                    msg.Reader().ReadUTF();
                    var b7 = msg.Reader().ReadByte();
                    if (b7 == 0) continue;
                    for (var n = 0; n < b7; n++)
                    {
                        msg.Reader().ReadUnsignedByte();
                        msg.Reader().ReadUnsignedShort();
                    }
                }

                var sizebox = msg.Reader().ReadByte();
                for (var num11 = 0; num11 < sizebox; num11++)
                {
                    var num12 = msg.Reader().ReadShort();
                    if (num12 == -1) continue;
                    msg.Reader().ReadInt();
                    msg.Reader().ReadUTF();
                    msg.Reader().ReadUTF();
                    var var4 = msg.Reader().ReadByte();
                    for (var num13 = 0; num13 < var4; num13++)
                    {
                        msg.Reader().ReadUnsignedByte();
                        msg.Reader().ReadUnsignedShort();
                    }
                }

                var num16 = msg.Reader().ReadShort();
                for (var num17 = 0; num17 < num16; num17++)
                {
                    msg.Reader().ReadShort();
                    msg.Reader().ReadShort();
                }

                msg.Reader().ReadShort();
                msg.Reader().ReadShort();
                msg.Reader().ReadShort();
                msg.Reader().ReadByte();
                msg.Reader().ReadByte();
                try
                {
                    msg.Reader().ReadShort();
                    msg.Reader().ReadSByte();
                    msg.Reader().ReadShort();
                }
                catch
                {
                }

                break;
            case 9:
                var plInMap = Session.Player.Map.FindCharInMap(msg.Reader().ReadInt());
                if (plInMap != null)
                {
                    plInMap.Hp = msg.Reader().ReadInt();
                    plInMap.HpFull = msg.Reader().ReadInt();
                }

                break;
            case 4:
                msg.Reader().ReadLong();
                msg.Reader().ReadInt();
                Session.Player.Hp = msg.Reader().ReadInt();
                Session.Player.Mp = msg.Reader().ReadInt();
                msg.Reader().ReadInt();
                break;
            case 15:
                @char = Session.Player.Map.FindCharInMap(msg.Reader().ReadInt());
                if (@char != null)
                {
                    @char.Hp = msg.Reader().ReadInt();
                    @char.HpFull = msg.Reader().ReadInt();
                    @char.X = msg.Reader().ReadShort();
                    @char.Y = msg.Reader().ReadShort();
                }

                break;
            case 5:
                Session.Player.Hp = msg.Reader().ReadInt();
                break;
            case 6:
                Session.Player.Mp = msg.Reader().ReadInt();
                break;
            case 10:
            case 11:
            case 12:
                @char = Session.Player.Map.FindCharInMap(msg.Reader().ReadInt());
                if (@char != null)
                {
                    @char.Hp = msg.Reader().ReadInt();
                    @char.HpFull = msg.Reader().ReadInt();
                    msg.Reader().ReadShort();
                    msg.Reader().ReadShort();
                    msg.Reader().ReadShort();
                }

                break;
            case 13:
                var num2 = msg.Reader().ReadInt();
                @char = num2 != Session.Player.CharId ? Session.Player.Map.FindCharInMap(num2) : Session.Player;
                if (@char != null)
                {
                    @char.Hp = msg.Reader().ReadInt();
                    @char.HpFull = msg.Reader().ReadInt();
                    msg.Reader().ReadShort();
                    msg.Reader().ReadShort();
                }

                break;
            case 14:
                @char = Session.Player.Map.FindCharInMap(msg.Reader().ReadInt());
                if (@char != null)
                {
                    @char.Hp = msg.Reader().ReadInt();
                    msg.Reader().ReadByte();
                    try
                    {
                        @char.HpFull = msg.Reader().ReadInt();
                    }
                    catch
                    {
                    }
                }

                break;
        }
    }

    private bool ReadCharInfo(Player c, Message msg)
    {
        try
        {
            msg.Reader().ReadByte();
            msg.Reader().ReadBool();
            msg.Reader().ReadByte();
            msg.Reader().ReadByte();
            c.Gender = msg.Reader().ReadByte();
            msg.Reader().ReadShort();
            msg.Reader().ReadUTF();
            c.Hp = msg.Reader().ReadInt();
            c.HpFull = msg.Reader().ReadInt();
            msg.Reader().ReadShort();
            msg.Reader().ReadShort();
            msg.Reader().ReadUnsignedByte();

            msg.Reader().ReadByte();

            c.X = msg.Reader().ReadShort();
            c.Y = msg.Reader().ReadShort();
            msg.Reader().ReadShort();
            msg.Reader().ReadShort();
            int num = msg.Reader().ReadByte();
            for (var i = 0; i < num; i++)
            {
                msg.Reader().ReadByte();
                msg.Reader().ReadInt();
                msg.Reader().ReadInt();
                msg.Reader().ReadShort();
            }

            return true;
        }
        catch
        {
        }

        return false;
    }

    public void LoadInfoMap(Message msg)
    {
        try
        {
            Session.Player.X = msg.Reader().ReadShort();
            Session.Player.Y = msg.Reader().ReadShort();
            int num = msg.Reader().ReadByte();
            Session.Player.Map.Waypoints.Clear();
            for (var i = 0; i < num; i++)
            {
                var waypoint = new Waypoint(msg.Reader().ReadShort(), msg.Reader().ReadShort(),
                    msg.Reader().ReadShort(), msg.Reader().ReadShort(), msg.Reader().ReadBool(),
                    msg.Reader().ReadBool(), msg.Reader().ReadUTF());
                if (Session.Player.Map.MapId is 21 or 22 or 23 && waypoint.MinX >= 0)
                {
                    _ = waypoint.MinX;
                    _ = 24;
                }

                Session.Player.Map.Waypoints.Add(waypoint);
            }

            num = msg.Reader().ReadByte();
            for (sbyte b = 0; b < num; b++)
            {
                var mob = new Mob(b, msg.Reader().ReadBool(), msg.Reader().ReadBool(), msg.Reader().ReadBool(),
                    msg.Reader().ReadBool(), msg.Reader().ReadBool(), msg.Reader().ReadByte(),
                    msg.Reader().ReadByte(), msg.Reader().ReadInt(), msg.Reader().ReadByte(), msg.Reader().ReadInt(),
                    msg.Reader().ReadShort(), msg.Reader().ReadShort(), msg.Reader().ReadByte(),
                    msg.Reader().ReadByte());
                msg.Reader().ReadBool();
                Session.Player.Map.Mobs.Add(mob);
            }

            num = msg.Reader().ReadByte();
            num = msg.Reader().ReadByte();
            for (var k = 0; k < num; k++)
            {
                msg.Reader().ReadByte();
                msg.Reader().ReadShort();
                msg.Reader().ReadShort();
                msg.Reader().ReadByte();
                msg.Reader().ReadShort();
            }

            num = msg.Reader().ReadByte();
            for (var l = 0; l < num; l++)
            {
                var itemMapID = msg.Reader().ReadShort();
                var num4 = msg.Reader().ReadShort();
                int x = msg.Reader().ReadShort();
                int y = msg.Reader().ReadShort();
                var num5 = msg.Reader().ReadInt();
                short r = 0;
                if (num5 == -2) r = msg.Reader().ReadShort();
                var itemMap = new ItemMap(num5, itemMapID, num4, x, y, r);
                var flag = false;
                for (var m = 0; m < Session.Player.Map.ItemMaps.Count; m++)
                    if (Session.Player.Map.ItemMaps[m].ItemMapId == itemMap.ItemMapId)
                    {
                        flag = true;
                        break;
                    }

                if (!flag) Session.Player.Map.ItemMaps.Add(itemMap);
            }

            var num14 = msg.Reader().ReadShort();
            for (var num15 = 0; num15 < num14; num15++)
            {
                msg.Reader().ReadShort();
                msg.Reader().ReadShort();
                msg.Reader().ReadShort();
            }

            var num16 = msg.Reader().ReadShort();
            for (var num17 = 0; num17 < num16; num17++)
            {
                msg.Reader().ReadUTF();
                msg.Reader().ReadUTF();
            }

            msg.Reader().ReadByte();
            msg.Reader().ReadByte();
            for (var i = 0; i < Session.Player.Map.Tmw * Session.Player.Map.Tmh; i++)
            for (var j = 0; j < Session.Player.Map.TileType[num].Length; j++)
                SetTile(i, Session.Player.Map.TileIndex[num][j], Session.Player.Map.TileType[num][j]);
        }
        catch
        {
        }
    }

    private void SetTile(int index, int[] mapsArr, int type)
    {
        for (var i = 0; i < mapsArr.Length; i++)
            if (Session.Player.Map.Maps[index] == mapsArr[i])
            {
                Session.Player.Map.Types[index] |= type;
                break;
            }
    }
}