using System.Collections.Generic;
using CreateSkillNro.Options;

namespace CreateSkillNro.Skills;

public class SkillPaint
{
    public short EffectHappenOnMob;

    public short Id;
    public PanelView PanelView;
    public SkillRender SkillRender;

    public List<SkillInfoPaint> SkillStand;
    public List<SkillInfoPaint> SkillFly;

    public void Dispose()
    {
        SkillRender?.Dispose();
        SkillStand.Clear();
        SkillFly.Clear();
        SkillFly = null;
        SkillRender = null;
        SkillStand = null;
        PanelView = null;
    }
}