namespace BotPlayer.Client;

public class Message
{
    private readonly myReader _dis;

    private readonly myWriter _dos;
    public sbyte Command;

    public Message(int command)
    {
        Command = (sbyte)command;
        _dos = new myWriter();
    }

    public Message()
    {
        _dos = new myWriter();
    }

    public Message(sbyte command)
    {
        Command = command;
        _dos = new myWriter();
    }

    public Message(sbyte command, sbyte[] data)
    {
        Command = command;
        _dis = new myReader(data);
    }

    public sbyte[] GetData()
    {
        return _dos.GetData();
    }

    public myReader Reader()
    {
        return _dis;
    }

    public myWriter Writer()
    {
        return _dos;
    }

    public void Cleanup()
    {
        _dos.Close();
        _dis.Close();
    }
}