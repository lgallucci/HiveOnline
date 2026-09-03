namespace HiveLib.Rules;

public sealed class MoveResult
{
    private MoveResult(bool isValid, string errorCode, bool isPlacement, bool gameEnded)
    {
        IsValid = isValid;
        ErrorCode = errorCode;
        IsPlacement = isPlacement;
        GameEnded = gameEnded;
    }

    public bool IsValid { get; }
    public string ErrorCode { get; }
    public bool IsPlacement { get; }
    public bool GameEnded { get; }

    public static MoveResult Accepted(bool isPlacement, bool gameEnded) =>
        new MoveResult(true, string.Empty, isPlacement, gameEnded);

    public static MoveResult Rejected(string errorCode) =>
        new MoveResult(false, errorCode, false, false);
}
