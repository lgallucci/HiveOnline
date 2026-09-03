using HiveContracts;

namespace HiveLib.Rules;

public readonly record struct HiveMove(
    BugTeam Team,
    BugType BugType,
    Hex From,
    Hex To);
