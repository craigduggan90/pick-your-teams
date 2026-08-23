using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Teams.Api.Controllers.V1.Users;
using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Users;
using Teams.Core.UseCases.Users.CreateUser;
using Teams.Core.UseCases.Users.DeleteUser;
using Teams.Core.UseCases.Users.GetSelf;
using Teams.Core.UseCases.Users.GetUserByExternalId;
using Teams.Core.UseCases.Users.GetUserById;
using Teams.Core.UseCases.Users.GetUsers;
using Teams.Core.UseCases.Users.UpdateUser;
using Teams.Domain.Entities;
using Teams.Domain.Extensions;

namespace Teams.Api.UnitTests.Controllers.V1.Users;

/// <summary>
/// Controllers are really tested through integration tests, these really just serve to ensure that we're injecting
/// values into mappers correctly.
/// </summary>
public static class UsersControllerTests
{
    public abstract class UserControllerTestsBase
    {
        protected readonly IMediator Mediator = Substitute.For<IMediator>();

        private UsersController? _sut;

        protected UsersController GetOrCreateSut() =>
            _sut ??= new UsersController(Mediator)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

        protected static User GetUser(
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

        protected static void AssertResultValue<TResult, TValue>(IActionResult result, TValue expected)
            where TResult : ObjectResult
        {
            var objectResult = Assert.IsType<TResult>(result);
            var actual = Assert.IsType<TValue>(objectResult.Value);
            Assert.Equivalent(expected, actual);
        }
    }

    public class GetUsers : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            var request = new GetUsersRequestModel();
            IReadOnlyCollection<User> users = [GetUser(), GetUser(), GetUser()];

            Mediator.SendAsync(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
                .Returns(users);

            var expected = users.ToPagedList(UsersMapper.ToModel);

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetUsers(request, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, PagedList<UserModel>>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>());
        }
    }

    public class CreateUser : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnCreatedAtAction_WhenSuccess()
        {
            var requestModel = new CreateUserRequestModel(
                DisplayName: "Jane Smith",
                ExternalId: "auth0|test-external-id",
                Email: "jane.smith@example.com",
                Mobile: "+447700900123");

            var user = GetUser(
                displayName: requestModel.DisplayName,
                externalId: requestModel.ExternalId,
                email: requestModel.Email,
                mobile: requestModel.Mobile);

            Mediator.SendAsync(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
                .Returns(user);

            var expected = user.ToModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.CreateUser(requestModel, TestContext.Current.CancellationToken);

            var createdResult = Assert.IsType<CreatedAtActionResult>(rawResult);
            var actual = Assert.IsType<UserModel>(createdResult.Value);
            Assert.Equivalent(expected, actual);
            Assert.Equal(nameof(UsersController.GetUserById), createdResult.ActionName);
            Assert.Equal(user.Id, createdResult.RouteValues?["id"]);

            await Mediator.Received(1).SendAsync(
                Arg.Is<CreateUserCommand>(c =>
                    c.DisplayName == requestModel.DisplayName &&
                    c.ExternalId == requestModel.ExternalId &&
                    c.Email == requestModel.Email &&
                    c.Mobile == requestModel.Mobile),
                Arg.Any<CancellationToken>());
        }
    }

    public class GetSelf : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            var user = GetUser();
            var detail = new UserDetail(user, PendingInvitations: 2);

            Mediator.SendAsync(Arg.Any<GetSelfQuery>(), Arg.Any<CancellationToken>())
                .Returns(detail);

            var expected = detail.ToDetailedModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetSelf(TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, UserDetailModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Any<GetSelfQuery>(), Arg.Any<CancellationToken>());
        }
    }

    public class GetUserById : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            const string id = "test-id";
            var user = GetUser(id: id);
            var detail = new UserDetail(user, PendingInvitations: 0);

            Mediator.SendAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
                .Returns(detail);

            var expected = detail.ToDetailedModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetUserById(id, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, UserDetailModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Is<GetUserByIdQuery>(q => q.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class GetUserByExternalId : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            const string externalId = "auth0|test-external-id";
            var user = GetUser(externalId: externalId);

            Mediator.SendAsync(Arg.Any<GetUserByExternalIdQuery>(), Arg.Any<CancellationToken>())
                .Returns(user);

            var expected = user.ToModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetUserByExternalId(externalId, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, UserModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(
                Arg.Is<GetUserByExternalIdQuery>(q => q.ExternalId == externalId), Arg.Any<CancellationToken>());
        }
    }

    public class UpdateUser : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";
            var requestModel = new UpdateUserRequestModel("jane_smith", "Jane Smith", "jane.smith@example.com", "+447700900123");

            Mediator.SendAsync(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetUser(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.UpdateUser(id, requestModel, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(
                Arg.Is<UpdateUserCommand>(c =>
                    c.Id == id &&
                    c.Tag == requestModel.Tag &&
                    c.DisplayName == requestModel.DisplayName &&
                    c.Email == requestModel.Email &&
                    c.Mobile == requestModel.Mobile),
                Arg.Any<CancellationToken>());
        }
    }

    public class DeleteUser : UserControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";

            Mediator.SendAsync(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetUser(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.DeleteUser(id, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(Arg.Is<DeleteUserCommand>(c => c.Id == id), Arg.Any<CancellationToken>());
        }
    }
}