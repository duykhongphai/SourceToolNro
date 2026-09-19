using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotPlayer.Classes;
using BotPlayer.PlayerData;
using BotPlayer.Windows;

namespace BotPlayer.Client;

public class Session
{
    public readonly Account Account;
    private CancellationTokenSource _cancellationToken;
    private bool _connected;
    private sbyte _curR;
    private sbyte _curW;
    private NetworkStream _dataStream;
    private long _lastTimeSendChangeMap;
    private long _lastTimeSendChat;
    private long _lastTimeSendChatWorld;
    private long _lastTimeSendRequestEffect;
    private long _lastTimeSendRequestIcon;
    private Controller _messageHandler;
    private Sender _sender;

    private TcpClient _socket;

    public BinaryReader Dis;

    public BinaryWriter Dos;

    public bool GetKeyComplete;

    public sbyte[] Key;
    public bool NeedRegister;
    public Player Player;
    public bool SendTypeClient;
    public bool SuccessLogin;

    public Session(Account acc)
    {
        Initialize();
        Account = acc;
        Task.Run(RunUpdateLoopAccount);
    }

    private void Initialize()
    {
        _sender = new Sender(this);
        _messageHandler = new Controller(this);
        Player = new Player(this);
        _cancellationToken = new CancellationTokenSource();
    }

    private async Task RunUpdateLoopAccount()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected())
            {
                NetworkInit();
                await Task.Delay(1000, _cancellationToken.Token);
                continue;
            }

            if (IsConnected() && !_socket.Connected)
            {
                SendTypeClient = false;
                NeedRegister = false;
                SuccessLogin = false;
                GetKeyComplete = false;
                Connect2();
                await Task.Delay(1000, _cancellationToken.Token);
                continue;
            }

            if (!SendTypeClient)
            {
                Service.Instance.SetClientType(this);
                Service.Instance.ImageSource(this);
                await Task.Delay(50, _cancellationToken.Token);
                continue;
            }

            if (NeedRegister)
            {
                var gender = Function.NextInt(0, 3);
                Service.Instance.CreateChar(this, Function.GenerateString(9), gender,
                    Const.HairID[gender][Function.NextInt(0, 2)]);
                NeedRegister = false;
            }
            else if (!SuccessLogin)
            {
                Service.Instance.Login(this);
                await Task.Delay(500, _cancellationToken.Token);
                Service.Instance.FinishUpdate(this);
                await Task.Delay(500, _cancellationToken.Token);
                Service.Instance.FinishLoadMap(this);
                await Task.Delay(1000, _cancellationToken.Token);
                Service.Instance.GetResource(this, 1);
            }
            else
            {
                UpdateFake();
            }

            await Task.Delay(300, _cancellationToken.Token);
        }
    }

    private void UpdateFake()
    {
        Player?.Update();
        Service.Instance.OpenUIZone(this);
        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTimeSendRequestIcon > 4000)
        {
            Service.Instance.RequestIcon(this, Function.NextInt(0, int.MaxValue));
            _lastTimeSendRequestIcon = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTimeSendRequestEffect > 26000)
        {
            Service.Instance.GetEffData(this, (short)Function.NextInt(0, short.MaxValue));
            _lastTimeSendRequestEffect = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTimeSendChat > 200)
        {
            Service.Instance.Chat(this,
                WindowExecution.Instance.IsChat ? Function.GenerateRandomChat() : "XTOOLS247.COM");
            _lastTimeSendChat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Function.NextInt(8000, 15000);
        }

        if (WindowExecution.Instance.IsChatGlobal &&
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTimeSendChatWorld > 2000)
        {
            Service.Instance.ChatGlobal(this, Function.GenerateRandomChat());
            _lastTimeSendChatWorld =
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Function.NextInt(180000, 420000);
        }

        if (WindowExecution.Instance.IsChangeMap &&
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTimeSendChangeMap >
            (WindowExecution.Instance.IsMonster ? 600000 : 60000))
        {
            Player?.AutoChangeRandomMap();
            _lastTimeSendChangeMap =
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        if (WindowExecution.Instance.IsMonster) Player?.AutoPlay();

        if (WindowExecution.Instance.IsPvp)
        {
            Player?.DoPvPNearPlayer();
        }
        else
        {
            if (Player?.Flag != 0) Service.Instance.ChooseFlag(this, 0);
        }
    }

    public bool IsConnected()
    {
        return _connected;
    }

    private void NetworkInit()
    {
        try
        {
            DoConnect();
        }
        catch
        {
            CleanNetwork();
        }
    }

    private void Connect2()
    {
        _socket?.Dispose();
        _dataStream?.Dispose();
        Dis?.Dispose();
        Dos?.Dispose();
        _socket = new TcpClient();
        _socket.Connect(WindowExecution.IpConnect!, WindowExecution.PortConnect);
        _dataStream = _socket.GetStream();
        Dis = new BinaryReader(_dataStream, new UTF8Encoding());
        Dos = new BinaryWriter(_dataStream, new UTF8Encoding());
        DoSendMessage(new Message(-27));
    }

    private void DoConnect()
    {
        _connected = true;
        _socket = new TcpClient();
        _socket.Connect(WindowExecution.IpConnect!, WindowExecution.PortConnect);
        _dataStream = _socket.GetStream();
        Dis = new BinaryReader(_dataStream, new UTF8Encoding());
        Dos = new BinaryWriter(_dataStream, new UTF8Encoding());
        MessageCollector collect = new(this);
        Task.Run(collect.Run);
        Task.Run(_sender.Run);
        DoSendMessage(new Message(-27));
    }

    public void SendMessage(Message message)
    {
        _sender.AddMessage(message);
    }

    public void DoSendMessage(Message m)
    {
        try
        {
            var data = m.GetData();
            if (GetKeyComplete)
            {
                var value = WriteKey(m.Command);
                Dos.Write(value);
            }
            else
            {
                Dos.Write(m.Command);
            }

            if (data != null)
            {
                var num = data.Length;
                if (GetKeyComplete)
                {
                    int num2 = WriteKey((sbyte)(num >> 8));
                    Dos.Write((sbyte)num2);
                    int num3 = WriteKey((sbyte)(num & 0xFF));
                    Dos.Write((sbyte)num3);
                }
                else
                {
                    Dos.Write((ushort)num);
                }

                if (GetKeyComplete)
                    for (var i = 0; i < data.Length; i++)
                    {
                        var value2 = WriteKey(data[i]);
                        Dos.Write(value2);
                    }
            }
            else
            {
                if (GetKeyComplete)
                {
                    var num4 = 0;
                    int num5 = WriteKey((sbyte)(num4 >> 8));
                    Dos.Write((sbyte)num5);
                    int num6 = WriteKey((sbyte)(num4 & 0xFF));
                    Dos.Write((sbyte)num6);
                }
                else
                {
                    Dos.Write((ushort)0);
                }
            }

            Dos.Flush();
            m.Cleanup();
        }
        catch
        {
        }
    }

    public sbyte ReadKey(sbyte b)
    {
        var array = Key;
        var num = _curR;
        _curR = (sbyte)(num + 1);
        var result = (sbyte)((array[num] & 0xFF) ^ (b & 0xFF));
        if (_curR >= Key.Length) _curR %= (sbyte)Key.Length;
        return result;
    }

    private sbyte WriteKey(sbyte b)
    {
        var array = Key;
        if (array == null) return 0;
        var num = _curW;
        _curW = (sbyte)(num + 1);
        var result = (sbyte)((array[num] & 0xFF) ^ (b & 0xFF));
        if (_curW >= Key.Length) _curW %= (sbyte)Key.Length;
        return result;
    }

    public void OnReceiveMsg(Message msg)
    {
        _messageHandler.OnMessage(msg);
    }

    public void CleanNetwork()
    {
        Key = null;
        _curR = 0;
        _curW = 0;
        try
        {
            _connected = false;
            Player?.Dispose();
            _cancellationToken?.Cancel();
            SendTypeClient = false;
            SuccessLogin = false;
            GetKeyComplete = false;
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            if (_dataStream != null)
            {
                _dataStream.Close();
                _dataStream = null;
            }

            if (Dos != null)
            {
                Dos.Close();
                Dos = null;
            }

            if (Dis == null) return;
            Dis.Close();
            Dis = null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}