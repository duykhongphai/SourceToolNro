using System.Collections.Generic;
using CreateSkillNro.Options;

namespace CreateSkillNro.Skills;

public class EffectCharPaint
{
    public List<EffectInfoPaint> ArrEffInfo;
    public PanelView PanelView;

    public void Dispose()
    {
        ArrEffInfo.Clear();
        PanelView = null;
        ArrEffInfo = null;
    }
}