namespace HiveContracts
{
    public interface ITile
    {
        BugType Type { get; set; }
        Hex Location { get; set; }
    }
}
