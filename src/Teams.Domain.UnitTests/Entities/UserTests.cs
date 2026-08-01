using Teams.Domain.Entities;
using Teams.Domain.UnitTests.TestHelpers;

namespace Teams.Domain.UnitTests.Entities;

public static class UserTests
{
    public abstract class UserTestsBase
    {
        protected const string DefaultTag = "tag";
        protected const string DefaultDisplayName = "display-name";
        protected const string DefaultExternalId = "external-id-001";
        protected const string DefaultEmailAddress = "user@example.com";
        protected const string DefaultMobile = "+15551234567";

        protected static User CreateUser(Action<User>? setup = null)
        {
            var user = new User(DefaultTag, DefaultDisplayName, DefaultExternalId, DefaultEmailAddress, DefaultMobile);
            setup?.Invoke(user);
            return user;
        }
    }

    public class Constructor : UserTestsBase
    {
        [Fact]
        public void CreatesUser_FromParameters()
        {
            var user = CreateUser();

            Assert.Equal(DefaultTag, user.Tag);
            Assert.Equal(DefaultDisplayName, user.DisplayName);
            Assert.Equal(DefaultExternalId, user.ExternalId);
            Assert.Equal(DefaultEmailAddress, user.EmailAddress);
            Assert.Equal(DefaultMobile, user.Mobile);
            Assert.Equal(1000, user.Rating);
            Assert.Empty(user.Participation);
        }

        [Fact]
        public void AllowsNullMobile()
        {
            var user = new User(DefaultTag, DefaultDisplayName, DefaultExternalId, DefaultEmailAddress, null);

            Assert.Null(user.Mobile);
        }
    }

    public class Update : UserTestsBase
    {
        [Fact]
        public void UpdatesTagEmailAndMobile_WhenAllProvided()
        {
            var user = CreateUser();

            user.Update("new-tag", "new@example.com", "+15559876543");

            Assert.Equal("new-tag", user.Tag);
            Assert.Equal("new@example.com", user.EmailAddress);
            Assert.Equal("+15559876543", user.Mobile);
        }

        [Fact]
        public void LeavesValuesUnchanged_WhenArgumentsAreNull()
        {
            var user = CreateUser();

            user.Update(null, null, null);

            Assert.Equal(DefaultTag, user.Tag);
            Assert.Equal(DefaultEmailAddress, user.EmailAddress);
            Assert.Equal(DefaultMobile, user.Mobile);
        }

        [Fact]
        public void UpdatesOnlyProvidedFields_WhenPartiallySpecified()
        {
            var user = CreateUser();

            user.Update("new-tag", null, null);

            Assert.Equal("new-tag", user.Tag);
            Assert.Equal(DefaultEmailAddress, user.EmailAddress);
            Assert.Equal(DefaultMobile, user.Mobile);
        }
    }

    public class ApplyRatingChange : UserTestsBase
    {
        [Fact]
        public void IncreasesRating_WhenChangeIsPositive()
        {
            var user = CreateUser();

            user.ApplyRatingChange(50);

            Assert.Equal(1050, user.Rating);
        }

        [Fact]
        public void DecreasesRating_WhenChangeIsNegative()
        {
            var user = CreateUser();

            user.ApplyRatingChange(-50);

            Assert.Equal(950, user.Rating);
        }

        [Fact]
        public void AccumulatesRating_AcrossMultipleCalls()
        {
            var user = CreateUser();

            user.ApplyRatingChange(50);
            user.ApplyRatingChange(-20);

            Assert.Equal(1030, user.Rating);
        }

        [Fact]
        public void LeavesRatingUnchanged_WhenChangeIsZero()
        {
            var user = CreateUser();

            user.ApplyRatingChange(0);

            Assert.Equal(1000, user.Rating);
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