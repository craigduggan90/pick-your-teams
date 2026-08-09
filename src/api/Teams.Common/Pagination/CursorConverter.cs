using System.Globalization;
using System.Text;

namespace Teams.Common.Pagination;

public static class CursorConverter
{
    /// <summary>Tries to decode a cursor string into a numeric pointer.</summary>
    /// <param name="input">The string to decode.</param>
    /// <param name="cursor">Contains the decoded result. Null if input is null or decoding failed.</param>
    /// <returns><c>True</c> if <c>input</c> was successfully decoded, otherwise <c>False</c>.</returns>
    public static bool TryDecodeCursor(this string? input, out long? cursor)
    {
        cursor = null;
        if (input is null)
            return true;

        try
        {
            var bytes = Convert.FromBase64String(input);
            var utf8 = Encoding.UTF8.GetString(bytes);

            cursor = long.TryParse(utf8, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;

            return cursor is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Tries to encode a numeric pointer into a cursor string.</summary>
    /// <param name="input">The numeric pointer to encode.</param>
    /// <param name="cursor">Contains the encoded result.  Null if input is null or encoding failed.</param>
    /// <returns><c>True</c> if <c>input</c> was successfully encoded, otherwise <c>False</c>.</returns>
    public static bool TryEncodeCursor(this long? input, out string? cursor)
    {
        cursor = null;
        if (input is null)
            return true;

        var bytes = Encoding.UTF8.GetBytes(input.Value.ToString(CultureInfo.InvariantCulture));
        cursor = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        return true;
    }
}