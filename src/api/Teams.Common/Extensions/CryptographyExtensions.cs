using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Teams.Common.Extensions;

/// <summary>Extension methods providing standardised implementations opf cryptographic functions.</summary>
public static class CryptographyExtensions
{
    /// <summary>Generate SHA-512 hash value for an object.</summary>
    /// <param name="input">The object for which to generate a digest hash.</param>
    /// <returns>The SHA-512 digest value for the given input.</returns>
    /// <remarks>
    /// The SHA algorithm is relatively expensive, so this should be used in applications where security is of greater
    /// concern than performance.  Note that SHA-512 is <b>not</b> cryptographically secure and shouldn't be used for
    /// encryption.
    /// </remarks>
    public static string GetShaDigest(this object input)
        => input.ToByteArray().GetChecksum(SHA512.Create);

    /// <summary>Generate SHA-512 hash value for an object.</summary>
    /// <param name="input">The object for which to generate a digest hash.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The SHA-512 digest value for the given input.</returns>
    /// <remarks>
    /// The SHA algorithm is relatively expensive, so this should be used in applications where security is of greater
    /// concern than performance.  Note that SHA-512 is <b>not</b> cryptographically secure and shouldn't be used for
    /// encryption.
    /// </remarks>
    public static Task<string> GetShaDigestAsync(this object input, CancellationToken cancellationToken = default)
        => input.ToByteArray().GetChecksumAsync(SHA512.Create, cancellationToken);

    /// <summary>Generate MD5 hash value for an object.</summary>
    /// <param name="input">The object for which to generate a digest hash.</param>
    /// <returns>The MD5 digest value for the given input.</returns>
    /// <remarks>
    /// The MD5 algorithm is relatively insecure, so this should be used in applications where performance is of greater
    /// concern than security.  Note that the MD5 algorithm is <b>not</b> cryptographically secure and shouldn't be used
    /// for encryption.
    /// </remarks>
    public static string GetMd5Digest(this object input)
        => input.ToByteArray().GetChecksum(MD5.Create);

    /// <summary>Generate MD5 hash value for an object.</summary>
    /// <param name="input">The object for which to generate a digest hash.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The MD5 digest value for the given input.</returns>
    /// <remarks>
    /// The MD5 algorithm is relatively insecure, so this should be used in applications where performance is of greater
    /// concern than security.  Note that the MD5 algorithm is <b>not</b> cryptographically secure and shouldn't be used
    /// for encryption.
    /// </remarks>
    public static Task<string> GetMd5DigestAsync(this object input, CancellationToken cancellationToken = default)
        => input.ToByteArray().GetChecksumAsync(MD5.Create, cancellationToken);

    /*
     * Private methods
     */

    private static byte[] ToByteArray(this object input)
        => Encoding.UTF8.GetBytes(input as string ?? input.Serialize());

    private static string GetChecksum(this byte[] input, Func<HashAlgorithm> providerFactory)
    {
        using var provider = providerFactory();
        var hashBytes = provider.ComputeHash(input);

        var builder = new StringBuilder();
        foreach (var b in hashBytes)
            builder.Append(b.ToString("x2", CultureInfo.InvariantCulture).ToLowerInvariant());

        return builder.ToString();
    }

    private static async Task<string> GetChecksumAsync(
        this byte[] input,
        Func<HashAlgorithm> providerFactory,
        CancellationToken cancellationToken = default)
    {
        using var provider = providerFactory();
        using var inputStream = new MemoryStream(input);
        var hashBytes = await provider.ComputeHashAsync(inputStream, cancellationToken).ConfigureAwait(false);

        var builder = new StringBuilder();
        foreach (var b in hashBytes)
            builder.Append(b.ToString("x2", CultureInfo.InvariantCulture).ToLowerInvariant());

        return builder.ToString();
    }
}