using System.Collections.Generic;
using CreateSkillNro.Options;

namespace CreateSkillNro.Skills;

public class DartInfo
{
    public List<List<short>> Head;

    public List<List<short>> HeadBorder;
    public short NUpdate;
    public PanelView PanelView;
    public PlayerDart PlayerDart;
    public List<short> Tail;

    public List<short> TailBorder;

    public int Va;

    public List<short> Xd1;

    public List<short> Xd2;

    public short XdPercent;

    public void Dispose()
    {
        Tail.Clear();
        TailBorder.Clear();
        Xd1.Clear();
        Xd2.Clear();
        PlayerDart?.Dispose();
        PlayerDart = null;
        PanelView = null;
        Head = null;
        HeadBorder = null;
        Tail = null;
        TailBorder = null;
        Xd1 = null;
        Xd2 = null;
    }
}