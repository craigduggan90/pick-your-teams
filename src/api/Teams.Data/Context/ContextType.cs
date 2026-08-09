namespace Teams.Data.Context;

/// <summary>Enum detailing the types of database context which can be constructed.</summary>
public enum ContextType
{
    /// <summary>Represents a context with read-only access.</summary>
    ReadOnly = 0,
    /// <summary>Represents a context with read-write permissions.</summary>
    ReadWrite = 1
}