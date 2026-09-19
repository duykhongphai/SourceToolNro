namespace CreateSkillNro.Skills;

public class EffectInfoPaint
{
    public sbyte Dx { get; set; }

    public sbyte Dy { get; set; }

    public short IdImg { get; set; }

    public EffectInfoPaint Clone()
    {
        return new EffectInfoPaint
        {
            Dx = Dx,
            Dy = Dy,
            IdImg = IdImg
        };
    }
}