namespace BotPlayer.PlayerData;

public class Mob
{
    public int hp;

    public bool isDie;

    public bool isDisable;

    public bool isDontMove;

    public bool isFire;

    public bool isFreez;

    public bool isIce;

    public bool isWind;

    public sbyte level;

    public sbyte levelBoss;

    public int maxHp;

    public int mobId;

    public string mobName;

    public int sys;

    public int templateId;

    private int wCount;

    public int x;

    public int y;


    public Mob(int mobId, bool isDisable, bool isDontMove, bool isFire, bool isIce, bool isWind, int templateId,
        int sys, int hp, sbyte level, int maxp, short pointx, short pointy, sbyte status, sbyte levelBoss)
    {
        this.isDisable = isDisable;
        this.isDontMove = isDontMove;
        this.isFire = isFire;
        this.isIce = isIce;
        this.isWind = isWind;
        this.sys = sys;
        this.mobId = mobId;
        this.templateId = templateId;
        this.hp = hp;
        this.level = level;
        x = pointx;
        y = pointy;
        maxHp = maxp;
        this.levelBoss = levelBoss;
        isDie = false;
    }
}