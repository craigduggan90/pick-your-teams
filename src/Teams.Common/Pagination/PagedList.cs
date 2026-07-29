namespace Teams.Common.Pagination;

public record PagedList<T>(IReadOnlyList<T> Data, string? Cursor, int Count);