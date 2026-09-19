using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotPlayer.Client;

public class Sender
{
    public Sender(Session session)
    {
        Session = session;
        SendingMessage = new Queue<Message>();
    }

    private Session Session { get; }
    private Queue<Message> SendingMessage { get; }

    public void AddMessage(Message message)
    {
        SendingMessage.Enqueue(message);
    }

    public async Task Run()
    {
        while (Session.IsConnected() && Session.Dos != null)
            try
            {
                if (Session.GetKeyComplete)
                    while (SendingMessage.Count > 0)
                    {
                        var m = SendingMessage.Dequeue();
                        if (m != null) Session.DoSendMessage(m);
                    }

                await Task.Delay(10);
            }
            catch
            {
            }
    }
}