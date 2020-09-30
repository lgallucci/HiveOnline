namespace HiveContracts
{
    public interface ITile
    {
        BugType Type { get; set; }
        BugTeam Team { get; set; }
        Hex Location { get; set; }

        bool CanMove(IBoard board);
    }
}
