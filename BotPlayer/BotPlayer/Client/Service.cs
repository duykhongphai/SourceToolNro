using BotPlayer.PlayerData;

namespace BotPlayer.Client;

public class Service
{
    public static Service Instance = new();

    public Message MessageNotLogin(sbyte command)
    {
        Message message = new(-29);
        message.Writer().WriteByte(command);
        return message;
    }

    public Message MessageNotMap(sbyte command)
    {
        Message message = new(-28);
        message.Writer().WriteByte(command);
        return message;
    }

    public static Message MessageSubCommand(sbyte command)
    {
        Message message = new(-30);
        message.Writer().WriteByte(command);
        return message;
    }

    public void ClientOk(Session session)
    {
        try
        {
            var message = MessageNotMap(13);
            session.SendMessage(message);
        }
        catch
        {
        }
    }


    public void OpenUIZone(Session session)
    {
        try
        {
            var message = new Message((sbyte)29);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void ReturnTownFromDead(Session session)
    {
        try
        {
            var message = new Message((sbyte)-15);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void GetResource(Session session, sbyte action)
    {
        try
        {
            Message message = new(-74);
            message.Writer().WriteByte(action);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void GetEffData(Session session, short id)
    {
        try
        {
            Message message = new(-66);
            message.Writer().WriteShort(id);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void RequestIcon(Session ss, int id)
    {
        try
        {
            Message message = new(-67);
            message.Writer().WriteInt(id);
            ss.SendMessage(message);
        }
        catch
        {
        }
    }


    public void SetClientType(Session session)
    {
        try
        {
            var message = MessageNotLogin(2);
            message.Writer().WriteByte(4);
            message.Writer().WriteByte(4);
            message.Writer().WriteBoolean(false);
            message.Writer().WriteInt(1024);
            message.Writer().WriteInt(768);
            message.Writer().WriteBoolean(true);
            message.Writer().WriteBoolean(true);
            message.Writer().WriteUTF("Pc platform xxx|2.3.7");
            session.SendMessage(message);
            session.SendTypeClient = true;
        }
        catch
        {
        }
    }

    public void Chat(Session session, string text)
    {
        try
        {
            var message = new Message((sbyte)44);
            message.Writer().WriteUTF(text);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void Login(Session session)
    {
        try
        {
            var message = MessageNotLogin(0);
            message.Writer().WriteUTF(session.Account.Username);
            message.Writer().WriteUTF(session.Account.Password);
            message.Writer().WriteUTF("2.3.7");
            message.Writer().WriteByte(0);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void UseItem(Session session, sbyte type, sbyte where, sbyte index, short template)
    {
        try
        {
            var message = new Message(-43);
            message.Writer().WriteByte(type);
            message.Writer().WriteByte(where);
            message.Writer().WriteByte(index);
            if (index == -1) message.Writer().WriteShort(template);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void FinishUpdate(Session session)
    {
        try
        {
            var message = new Message(-38);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void FinishLoadMap(Session session)
    {
        try
        {
            var message = new Message(-39);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void RequestMapTemplate(Session session, int maptemplateId)
    {
        try
        {
            var message = MessageNotMap(10);
            message.Writer().WriteByte(maptemplateId);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void SendPlayerAttack(Session session, Mob vMob, Player vChar)
    {
        try
        {
            Message message = null;
            if (vMob != null)
            {
                message = new Message(54);
                message.Writer().WriteByte(vMob.mobId);
            }
            else if (vChar != null)
            {
                message = new Message((sbyte)-60);
                message.Writer().WriteInt(vChar.CharId);
            }

            if (message == null) return;
            var num = (vMob?.x ?? vChar.X) - session.Player.X;
            message.Writer().WriteSByte((sbyte)(num > 0 ? 1 : -1));
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void GetMapOffline(Session session)
    {
        try
        {
            var message = new Message(-33);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void RequestChangeMap(Session session)
    {
        var message = new Message(-23);
        session.SendMessage(message);
    }

    public void OpenMenu(Session session, int npcId)
    {
        try
        {
            var message = new Message(33);
            message.Writer().WriteShort((short)npcId);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void Menu(Session session, int npcId, int menuId, int optionId)
    {
        try
        {
            var message = new Message(22);
            message.Writer().WriteByte(npcId);
            message.Writer().WriteByte(menuId);
            message.Writer().WriteByte(optionId);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void PickItem(Session session, int itemMapId)
    {
        try
        {
            var message = new Message(-20);
            message.Writer().WriteShort((short)itemMapId);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void RequestChangeZone(Session session, int zoneId)
    {
        try
        {
            var message = new Message((sbyte)21);
            message.Writer().WriteByte(zoneId);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void CharMove(Session session, short x, short y)
    {
        try
        {
            session.Player.X = x;
            session.Player.Y = y;
            var message = new Message((sbyte)-7);
            if (session.Player.Map.TileTypeAt(x / 24, y / 24) == 0)
                message.Writer().WriteByte((sbyte)1);
            else
                message.Writer().WriteByte((sbyte)0);
            message.Writer().WriteShort(x);
            message.Writer().WriteShort(y);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void SelectSkill(Session session, int skillTemplateId)
    {
        try
        {
            var message = new Message((sbyte)34);
            message.Writer().WriteShort((short)skillTemplateId);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void ChatGlobal(Session session, string text)
    {
        try
        {
            var message = new Message((sbyte)-71);
            message.Writer().WriteUTF(text);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void ChooseFlag(Session session, sbyte flagType)
    {
        try
        {
            var message = new Message((sbyte)-103);
            message.Writer().WriteByte(1);
            message.Writer().WriteByte(flagType);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void ChatPlayer(Session session, string text, int id)
    {
        try
        {
            var message = new Message((sbyte)-72);
            message.Writer().WriteInt(id);
            message.Writer().WriteUTF(text);
            session.SendMessage(message);
        }
        catch
        {
        }
    }

    public void ImageSource(Session session)
    {
        try
        {
            Message message = new(-111);
            message.Writer().WriteShort(0);
            session.SendMessage(message);
        }
        catch
        {
        }
    }


    public void CreateChar(Session session, string name, int gender, int hair)
    {
        try
        {
            Message message = new(-28);
            message.Writer().WriteByte((sbyte)2);
            message.Writer().WriteUTF(name);
            message.Writer().WriteByte(gender);
            message.Writer().WriteByte(hair);
            session.SendMessage(message);
        }
        catch
        {
        }
    }
}