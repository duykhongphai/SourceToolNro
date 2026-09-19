using System;
using System.Threading.Tasks;

namespace BotPlayer.Client;

public class MessageCollector
{
    private readonly Session _session;

    public MessageCollector(Session session)
    {
        _session = session;
    }

    public async Task Run()
    {
        try
        {
            while (_session.IsConnected())
            {
                var message = ReadMessage();
                if (message != null)
                {
                    if (message.Command == -27)
                        GetKey(message);
                    else
                        _session.OnReceiveMsg(message);
                }

                await Task.Delay(10);
            }
        }
        catch
        {
        }
    }

    private void GetKey(Message message)
    {
        try
        {
            var b = message.Reader().ReadSByte();
            _session.Key = new sbyte[b];
            for (var i = 0; i < b; i++) _session.Key[i] = message.Reader().ReadSByte();
            for (var j = 0; j < _session.Key.Length - 1; j++)
            {
                ref var reference = ref _session.Key[j + 1];
                reference ^= _session.Key[j];
            }

            _session.GetKeyComplete = true;
        }
        catch (Exception)
        {
        }
    }

    private Message ReadMessage2(sbyte cmd)
    {
        var num = _session.ReadKey(_session.Dis.ReadSByte()) + 128;
        var num2 = _session.ReadKey(_session.Dis.ReadSByte()) + 128;
        var num3 = _session.ReadKey(_session.Dis.ReadSByte()) + 128;
        var num4 = (num3 * 256 + num2) * 256 + num;
        var array = new sbyte[num4];
        var src = _session.Dis.ReadBytes(num4);
        Buffer.BlockCopy(src, 0, array, 0, num4);
        if (!_session.GetKeyComplete) return new Message(cmd, array);
        for (var i = 0; i < array.Length; i++)
            array[i] = _session.ReadKey(array[i]);

        return new Message(cmd, array);
    }

    private Message ReadMessage()
    {
        try
        {
            var b = _session.Dis.ReadSByte();
            if (_session.GetKeyComplete) b = _session.ReadKey(b);
            if (b == -32 || b == -66 || b == 11 || b == -67 || b == -74 || b == -87 || b == 66) return ReadMessage2(b);
            int num;
            if (_session.GetKeyComplete)
            {
                var b2 = _session.Dis.ReadSByte();
                var b3 = _session.Dis.ReadSByte();
                num = ((_session.ReadKey(b2) & 0xFF) << 8) | (_session.ReadKey(b3) & 0xFF);
            }
            else
            {
                var b4 = _session.Dis.ReadSByte();
                var b5 = _session.Dis.ReadSByte();
                num = (b4 & 0xFF00) | (b5 & 0xFF);
            }

            var array = new sbyte[num];
            var src = _session.Dis.ReadBytes(num);
            Buffer.BlockCopy(src, 0, array, 0, num);
            if (!_session.GetKeyComplete) return new Message(b, array);
            for (var i = 0; i < array.Length; i++)
                array[i] = _session.ReadKey(array[i]);

            return new Message(b, array);
        }
        catch
        {
        }

        return null;
    }
}