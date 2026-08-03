namespace Teams.Domain.UnitTests.TestHelpers;

public static class ObjectExtensions
{
    public static object? GetValue(this object obj, Type type, string propertyName)
        => type.GetProperty(propertyName)?.GetValue(obj);
}