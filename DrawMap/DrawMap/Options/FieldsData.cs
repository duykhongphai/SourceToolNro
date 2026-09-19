namespace DrawMap.Options;

public class FieldsData
{
    public int Id { get; init; }
    public int Dx { get; set; }
    public int Dy { get; set; }
    public int Layer { get; set; }
    public int IdImage { get; init; }
    public int IdParent { get; init; }
    public byte BackgroundId { get; init; }
    public byte BackgroundType { get; set; }
    public short HeadId { get; init; }
    public short BodyId { get; init; }
    public short LegId { get; init; }
}