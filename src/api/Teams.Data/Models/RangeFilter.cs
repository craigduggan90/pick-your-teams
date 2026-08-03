namespace Teams.Data.Models;

public record RangeFilter<T>(T? From, T? To)
    where T : struct;