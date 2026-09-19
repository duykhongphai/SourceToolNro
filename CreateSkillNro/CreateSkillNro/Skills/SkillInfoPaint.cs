namespace CreateSkillNro.Skills;

public class SkillInfoPaint
{
    public int Adx;

    public int Ady;

    public short ArrowId;

    public short E0dx;

    public short E0dy;

    public short E1dx;

    public short E1dy;

    public short E2dx;

    public short E2dy;

    public short EffS0Id;

    public short EffS1Id;

    public short EffS2Id;
    public sbyte Status;

    public SkillInfoPaint Clone()
    {
        return new SkillInfoPaint
        {
            Adx = Adx,
            Ady = Ady,
            ArrowId = ArrowId,
            E0dx = E0dx,
            E0dy = E0dy,
            E1dx = E1dx,
            E1dy = E1dy,
            E2dx = E2dx,
            E2dy = E2dy,
            EffS0Id = EffS0Id,
            EffS1Id = EffS1Id,
            EffS2Id = EffS2Id,
            Status = Status
        };
    }
}