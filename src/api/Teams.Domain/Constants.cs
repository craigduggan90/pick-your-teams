namespace Teams.Domain;

public static class Constants
{
    public const int StartingElo = 1000;

    /// <summary>
    /// ELO scaling constant controlling how a ratings gap between two teams
    /// translates into win probability. Standard chess value, inherited here as
    /// a starting point — a 400-point gap implies the stronger side is expected
    /// to win roughly 91% of the time. The win-probability display doubles as a
    /// calibration check: if shown percentages consistently feel wrong for how
    /// games actually play out, this is the value to revisit.
    /// </summary>
    public const double EloScalingFactor = 400.0;

    /// <summary>
    /// ELO K-factor: the maximum number of rating points a single result can move
    /// a team's expected-score delta by. Higher K means each game has more impact
    /// on rating; lower K means ratings change more gradually. Set toward the
    /// higher end of chess's typical 16-40 range, since games here are infrequent
    /// per player and each result should carry more weight than a single chess game would.
    /// </summary>
    public const int EloK = 32;
}