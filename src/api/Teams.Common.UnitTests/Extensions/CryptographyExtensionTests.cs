using Teams.Common.Extensions;

namespace Teams.Common.UnitTests.Extensions;

public class CryptographyExtensionTests
{
    /*
     * GetShaDigest
     */

    [Fact]
    public void GetShaDigest_ReturnsExpectedValue_ForInputString()
    {
        const string input = "example input string";
        const string expected = "c4ce8f6c4f97dd15c50694e9336f2c763b5f15d966232a77257a913925cb5875da97c318c6066d63b57fbc6d524b73089131391631f63f32e19a263fc8ecbcf3";

        var actual = input.GetShaDigest();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetShaDigest_ReturnsSameValue_ForObject_AndSerializedString()
    {
        // We do this check because it means that if both the checksum publisher and recipient use the common package,
        // then they should get the same result regardless of whether they check against the deserializes object or the
        // string from which it was deserialized.

        var objectInput = new
        {
            Property = "Value",
            Numero = 1,
            IsTrue = false
        };
        var stringInput = objectInput.Serialize();

        var objectHash = objectInput.GetShaDigest();
        var stringHash = stringInput.GetShaDigest();
        Assert.Equal(objectHash, stringHash);
    }

    /*
     * GetShaDigestAsync
     */

    [Fact]
    public async Task GetShaDigestAsync_ReturnsExpectedValue_ForInputString()
    {
        const string input = "example input string";
        const string expected = "c4ce8f6c4f97dd15c50694e9336f2c763b5f15d966232a77257a913925cb5875da97c318c6066d63b57fbc6d524b73089131391631f63f32e19a263fc8ecbcf3";

        var actual = await input.GetShaDigestAsync(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetShaDigestAsync_ReturnsSameValue_ForObject_AndSerializedString()
    {
        var objectInput = new
        {
            Property = "Value",
            Numero = 1,
            IsTrue = false
        };
        var stringInput = objectInput.Serialize();

        var objectHash = await objectInput.GetShaDigestAsync(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(true);
        var stringHash = await stringInput.GetShaDigestAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.Equal(objectHash, stringHash);
    }

    [Fact]
    public async Task GetShaDigestAsync_ReturnsSameValue_AsGetShaDigest()
    {
        // We do this test to make sure that the sync & async methods are identical.
        const string input = "sync/async test input";

        // ReSharper disable once MethodHasAsyncOverload
        var syncHash = input.GetShaDigest();
        var asyncHash = await input.GetShaDigestAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.Equal(syncHash, asyncHash);
    }

    /*
     * GetMd5Digest
     */

    [Fact]
    public void GetMd5Digest_ReturnsExpectedValue_ForInputString()
    {
        const string input = "example input string";
        const string expected = "c8ceb42aba239a8910d97455b03be124";

        var actual = input.GetMd5Digest();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetMd5Digest_ReturnsSameValue_ForObject_AndSerializedString()
    {
        // We do this check because it means that if both the checksum publisher and recipient use the common package,
        // then they should get the same result regardless of whether they check against the deserializes object or the
        // string from which it was deserialized.

        var objectInput = new
        {
            Property = "Value",
            Numero = 1,
            IsTrue = false
        };
        var stringInput = objectInput.Serialize();

        var objectHash = objectInput.GetMd5Digest();
        var stringHash = stringInput.GetMd5Digest();
        Assert.Equal(objectHash, stringHash);
    }

    /*
     * GetMd5DigestAsync
     */

    [Fact]
    public async Task GetMd5DigestAsync_ReturnsExpectedValue_ForInputString()
    {
        const string input = "example input string";
        const string expected = "c8ceb42aba239a8910d97455b03be124";

        var actual = await input.GetMd5DigestAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetMd5DigestAsync_ReturnsSameValue_ForObject_AndSerializedString()
    {
        var objectInput = new
        {
            Property = "Value",
            Numero = 1,
            IsTrue = false
        };
        var stringInput = objectInput.Serialize();

        var objectHash = await objectInput.GetMd5DigestAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var stringHash = await stringInput.GetMd5DigestAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.Equal(objectHash, stringHash);
    }

    [Fact]
    public async Task GetMd5DigestAsync_ReturnsSameValue_AsGetMd5Digest()
    {
        // We do this test to make sure that the sync & async methods are identical.
        const string input = "sync/async test input";

        // ReSharper disable once MethodHasAsyncOverload
        var syncHash = input.GetMd5Digest();
        var asyncHash = await input.GetMd5DigestAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.Equal(syncHash, asyncHash);
    }
}