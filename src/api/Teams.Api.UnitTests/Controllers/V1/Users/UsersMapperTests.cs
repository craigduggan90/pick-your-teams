using Teams.Api.Controllers.V1.Users;
using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Domain.Entities;

namespace Teams.Api.UnitTests.Controllers.V1.Users;

public static class UsersMapperTests
{
    private static User GetUser(
        string? id = null,
        string displayName = "Test User",
        string? externalId = null,
        string email = "test@example.com",
        string? mobile = null)
    {
        using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
        return new User(
            displayName,
            externalId ?? Guid.NewGuid().ToString("N"),
            email,
            mobile);
    }

    public class ToModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var user = GetUser();

            var result = user.ToModel();

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Tag, result.Tag);
            Assert.Equal(user.DisplayName, result.DisplayName);
            Assert.Equal(user.Rating, result.Rating);
        }
    }

    public class ToDetailedModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var user = GetUser(email: "jane.smith@example.com", mobile: "+447700900123");

            var result = user.ToDetailedModel();

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Tag, result.Tag);
            Assert.Equal(user.DisplayName, result.DisplayName);
            Assert.Equal(user.Rating, result.Rating);
            Assert.Equal(user.EmailAddress, result.Email);
            Assert.Equal(user.Mobile, result.Mobile);
            Assert.Equal(user.DateCreated, result.Created);
            Assert.Equal(user.DateModified, result.Modified);
        }

        [Fact]
        public void SetsMobileToNull_WhenUserHasNoMobile()
        {
            var user = GetUser(mobile: null);

            var result = user.ToDetailedModel();

            Assert.Null(result.Mobile);
        }
    }

    public class ToCommandFromCreateUserRequestModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var model = new CreateUserRequestModel(
                DisplayName: "Jane Smith",
                ExternalId: "auth0|test-external-id",
                Email: "jane.smith@example.com",
                Mobile: "+447700900123");

            var result = model.ToCommand();

            Assert.Equal(model.DisplayName, result.DisplayName);
            Assert.Equal(model.ExternalId, result.ExternalId);
            Assert.Equal(model.Email, result.Email);
            Assert.Equal(model.Mobile, result.Mobile);
        }
    }

    public class ToCommandFromUpdateUserRequestModel
    {
        [Fact]
        public void MapsIdAndProvidedProperties_WhenCalled()
        {
            var model = new UpdateUserRequestModel("jane_smith", "Jane Smith", "jane.smith@example.com", "+447700900123");

            var result = model.ToCommand("test-user-id");

            Assert.Equal("test-user-id", result.Id);
            Assert.Equal(model.Tag, result.Tag);
            Assert.Equal(model.DisplayName, result.DisplayName);
            Assert.Equal(model.Email, result.Email);
            Assert.Equal(model.Mobile, result.Mobile);
        }

        [Fact]
        public void SetsPropertiesToNull_WhenModelPropertiesAreNull()
        {
            var model = new UpdateUserRequestModel(null, null, null, null);

            var result = model.ToCommand("test-user-id");

            Assert.Null(result.Tag);
            Assert.Null(result.DisplayName);
            Assert.Null(result.Email);
            Assert.Null(result.Mobile);
        }
    }

    public class ToQuery
    {
        [Fact]
        public void MapsAllSimpleProperties_WhenCalled()
        {
            var model = new GetUsersRequestModel(
                EmailAddress: "jane.smith@example.com",
                Tag: "jane_smith",
                DisplayName: "Jane Smith",
                RatingFrom: 900,
                RatingTo: 1100,
                CreatedFrom: new DateTime(2026, 1, 2),
                CreatedTo: new DateTime(2026, 1, 3),
                ModifiedFrom: new DateTime(2026, 1, 4),
                ModifiedTo: new DateTime(2026, 1, 5),
                PageSize: 25,
                Cursor: null);

            var result = model.ToQuery();

            Assert.Equal(model.EmailAddress, result.EmailAddress);
            Assert.Equal(model.Tag, result.Tag);
            Assert.Equal(model.DisplayName, result.DisplayName);
            Assert.Equal(model.RatingFrom, result.RatingFrom);
            Assert.Equal(model.RatingTo, result.RatingTo);
            Assert.Equal(model.CreatedFrom, result.CreatedFrom);
            Assert.Equal(model.CreatedTo, result.CreatedTo);
            Assert.Equal(model.ModifiedFrom, result.ModifiedFrom);
            Assert.Equal(model.ModifiedTo, result.ModifiedTo);
            Assert.Equal(model.PageSize, result.PageSize);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsNull()
        {
            var model = new GetUsersRequestModel(Cursor: null);

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsInvalid()
        {
            var model = new GetUsersRequestModel(Cursor: "not-a-valid-cursor!!");

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void DecodesCursor_WhenCursorIsValid()
        {
            ((long?)12345).TryEncodeCursor(out var encodedCursor);
            var model = new GetUsersRequestModel(Cursor: encodedCursor);

            var result = model.ToQuery();

            Assert.Equal(12345, result.Cursor);
        }
    }
}