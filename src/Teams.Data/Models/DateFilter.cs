namespace Teams.Data.Models;

/// <summary>Model containing filter values for base entity created and last modified timestamps.</summary>
/// <param name="Created">Limit results to entities created within this range</param>
/// <param name="Modified">Limit results to entities last modified within this range.</param>
public record DateFilter(
    RangeFilter<DateTime>? Created,
    RangeFilter<DateTime>? Modified);