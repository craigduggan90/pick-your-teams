using Teams.Common.Providers.Identifiers;
using Teams.Domain.Entities;
using Teams.Domain.UnitTests.TestHelpers;

namespace Teams.Domain.UnitTests.Entities;

public static class UserTests
{
    public abstract class UserTestsBase
    {
        protected const string DefaultDisplayName = "display-name";
        protected const string DefaultExternalId = "external-id-001";
        protected const string DefaultEmailAddress = "user@example.com";
        protected const string DefaultMobile = "+15551234567";
        protected const int DefaultRating = 1000;

        protected static User CreateUser(Action<User>? setup = null)
        {
            var user = new User(DefaultDisplayName, DefaultExternalId, DefaultEmailAddress, DefaultMobile);
            setup?.Invoke(user);
            return user;
        }
    }

    public class Constructor : UserTestsBase
    {
        [Fact]
        public void CreatesUser_FromParameters()
        {
            const string id = "test-identifier";
            using var _ = new IdentifierProviderContext(id);
            var user = CreateUser();

            Assert.Equal(id, user.Tag);
            Assert.Equal(DefaultDisplayName, user.DisplayName);
            Assert.Equal(DefaultExternalId, user.ExternalId);
            Assert.Equal(DefaultEmailAddress, user.EmailAddress);
            Assert.Equal(DefaultMobile, user.Mobile);
            Assert.Equal(DefaultRating, user.Rating);
            Assert.Empty(user.Participation);
        }

        [Fact]
        public void AllowsNullMobile()
        {
            var user = new User(DefaultDisplayName, DefaultExternalId, DefaultEmailAddress, null);

            Assert.Null(user.Mobile);
        }
    }

    public class Update : UserTestsBase
    {
        [Fact]
        public void UpdatesTagDisplayNameEmailAndMobile_WhenAllProvided()
        {
            const string expectedTag = "new-tag";
            const string expectedDisplayName = "new-display-name";
            const string expectedEmailAddress = "new@example.com";
            const string expectedMobile = "+15559876543";
            var user = CreateUser();

            user.Update(expectedTag, expectedDisplayName, expectedEmailAddress, expectedMobile);

            Assert.Equal(expectedTag, user.Tag);
            Assert.Equal(expectedDisplayName, user.DisplayName);
            Assert.Equal(expectedEmailAddress, user.EmailAddress);
            Assert.Equal(expectedMobile, user.Mobile);
        }

        [Fact]
        public void LeavesValuesUnchanged_WhenArgumentsAreNull()
        {
            var user = CreateUser();

            user.Update(null, null, null, null);

            Assert.Equal(user.Id, user.Tag);
            Assert.Equal(DefaultDisplayName, user.DisplayName);
            Assert.Equal(DefaultEmailAddress, user.EmailAddress);
            Assert.Equal(DefaultMobile, user.Mobile);
        }

        [Fact]
        public void UpdatesOnlyProvidedFields_WhenPartiallySpecified()
        {
            const string expectedTag = "new-tag";
            var user = CreateUser();

            user.Update(expectedTag, null, null, null);

            Assert.Equal(expectedTag, user.Tag);
            Assert.Equal(DefaultDisplayName, user.DisplayName);
            Assert.Equal(DefaultEmailAddress, user.EmailAddress);
            Assert.Equal(DefaultMobile, user.Mobile);
        }
    }

    public class ApplyRatingChange : UserTestsBase
    {
        [Fact]
        public void IncreasesRating_WhenChangeIsPositive()
        {
            const int change = 50;
            const int expectedRating = DefaultRating + change;
            var user = CreateUser();

            user.ApplyRatingChange(change);

            Assert.Equal(expectedRating, user.Rating);
        }

        [Fact]
        public void DecreasesRating_WhenChangeIsNegative()
        {
            const int change = -50;
            const int expectedRating = DefaultRating + change;
            var user = CreateUser();

            user.ApplyRatingChange(change);

            Assert.Equal(expectedRating, user.Rating);
        }

        [Fact]
        public void AccumulatesRating_AcrossMultipleCalls()
        {
            const int firstChange = 50;
            const int secondChange = -20;
            const int expectedRating = DefaultRating + firstChange + secondChange;
            var user = CreateUser();

            user.ApplyRatingChange(firstChange);
            user.ApplyRatingChange(secondChange);

            Assert.Equal(expectedRating, user.Rating);
        }

        [Fact]
        public void LeavesRatingUnchanged_WhenChangeIsZero()
        {
            var user = CreateUser();

            user.ApplyRatingChange(0);

            Assert.Equal(DefaultRating, user.Rating);
            Assert.False(user.IsDirty);
        }
    }

    public class AsSerializable : UserTestsBase
    {
        [Fact]
        public void IncludesIdAndTimestamps_WhenCalled()
        {
            var user = CreateUser();

            var serializable = user.AsSerializable();
            var type = serializable.GetType();

            Assert.Equal(user.Id, serializable.GetValue(type, "Id"));
            Assert.Equal(user.DateCreated, serializable.GetValue(type, "DateCreated"));
            Assert.Equal(user.DateModified, serializable.GetValue(type, "DateModified"));
        }

        [Fact]
        public void ExcludesTagDisplayNameExternalIdContactDetailsAndRating_WhenCalled()
        {
            var user = CreateUser();

            var serializable = user.AsSerializable();
            var type = serializable.GetType();

            Assert.Null(serializable.GetValue(type, "Tag"));
            Assert.Null(serializable.GetValue(type, "DisplayName"));
            Assert.Null(serializable.GetValue(type, "ExternalId"));
            Assert.Null(serializable.GetValue(type, "EmailAddress"));
            Assert.Null(serializable.GetValue(type, "Mobile"));
            Assert.Null(serializable.GetValue(type, "Rating"));
        }
    }
}