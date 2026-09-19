namespace BotPlayer.PlayerData.XMap;

public class XMapMain(Player player)
{
    private Player _player = player;
    public bool IsXMapRunning = false;

    public void Update()
    {
        if (_player.XMapData.IsLoading)
            _player.XMapData.Update();
        if (IsXMapRunning)
            _player.XMapController.Update();
    }

    public void Info(string text)
    {
        if (text.Equals("Bạn chưa thể đến khu vực này"))
            _player.XMapController.FinishXmap();
    }

    public void Dispose()
    {
        _player = null;
    }
}