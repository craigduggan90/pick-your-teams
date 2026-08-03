namespace Teams.Core.Exceptions;

public class ConcurrencyTokenMismatchException() : Exception("Concurrency Token does not match current record state.")
{
    public static void ThrowIfMismatch(string requestValue, string currentValue)
    {
        if (!requestValue.Equals(currentValue, StringComparison.OrdinalIgnoreCase))
            throw new ConcurrencyTokenMismatchException();
    }
}