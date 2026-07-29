namespace Teams.Data.Models;

/// <summary>Model containing filter values for base entity created and last modified timestamps.</summary>
/// <param name="CreatedFrom">Limit results to entities created on or after this date.</param>
/// <param name="CreatedTo">Limit results to entities created before this date.</param>
/// <param name="ModifiedFrom">Limit results to entities last modified on or after this date.</param>
/// <param name="ModifiedTo">Limit results to entities last modified before this date.</param>
public record DateFilter(
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null);