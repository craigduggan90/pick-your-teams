using Teams.Domain.Entities;

namespace Teams.Domain.Models;

public readonly record struct TeamSuggestion(
    IReadOnlyCollection<Player> Home,
    IReadOnlyCollection<Player> Away,
    int HomeRating,
    int AwayRating,
    int TeamDifferential);