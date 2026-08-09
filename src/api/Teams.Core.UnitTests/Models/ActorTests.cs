using Teams.Core.Exceptions;
using Teams.Core.Models;

namespace Teams.Core.UnitTests.Models;

public static class ActorTests
{
    private const string ActorId = "actor-001";

    private static Actor CreateActor(string id = ActorId) => new(id, "tag-001", "display-name");

    public class ThrowIfNotOrganiser
    {
        [Fact]
        public void DoesNotThrow_WhenActorIsOrganiser()
        {
            var actor = CreateActor();

            var exception = Record.Exception(() => actor.ThrowIfNotOrganiser(ActorId));

            Assert.Null(exception);
        }

        [Fact]
        public void DoesNotThrow_WhenActorIsOrganiser_IgnoringCase()
        {
            var actor = CreateActor("Actor-001");

            var exception = Record.Exception(() => actor.ThrowIfNotOrganiser(ActorId));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsAccessDeniedException_WhenActorIsNotOrganiser()
        {
            var actor = CreateActor("some-other-actor");

            Assert.Throws<AccessDeniedException>(() => actor.ThrowIfNotOrganiser(ActorId));
        }
    }

    public class ThrowIfNotUser
    {
        [Fact]
        public void DoesNotThrow_WhenActorIsUser()
        {
            var actor = CreateActor();

            var exception = Record.Exception(() => actor.ThrowIfNotUser(ActorId));

            Assert.Null(exception);
        }

        [Fact]
        public void DoesNotThrow_WhenActorIsUser_IgnoringCase()
        {
            var actor = CreateActor("Actor-001");

            var exception = Record.Exception(() => actor.ThrowIfNotUser(ActorId));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsAccessDeniedException_WhenActorIsNotUser()
        {
            var actor = CreateActor("some-other-actor");

            Assert.Throws<AccessDeniedException>(() => actor.ThrowIfNotUser(ActorId));
        }
    }
}