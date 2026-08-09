namespace Teams.Domain.Interfaces;

/// <summary>Describes an entity which supports cursor-based pagination.</summary>
public interface IHasCursor
{
    /// <summary>The entity cursor.</summary>
    long Cursor { get; }
}