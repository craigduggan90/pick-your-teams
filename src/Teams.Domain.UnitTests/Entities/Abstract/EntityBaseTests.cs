using Teams.Common.Extensions;
using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Domain.Entities.Abstract;
using Teams.Domain.Exceptions;

namespace Teams.Domain.UnitTests.Entities.Abstract;

public static class EntityBaseTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitializeProperties()
        {
            const string id = "test-id";
            var timestamp = new DateTime(2025, 5, 12, 12, 56, 2, DateTimeKind.Utc);
            var expectedCursor = (long)(timestamp - DateTimeOffset.UnixEpoch).TotalMicroseconds;

            using var idFix = new IdentifierProviderContext(id);
            using var dtFix = new DateTimeOffsetProviderContext(timestamp);
            var entity = new ExampleEntity();

            Assert.Equal(id, entity.Id);
            Assert.Equal(timestamp, entity.DateCreated);
            Assert.Equal(timestamp, entity.DateModified);
            Assert.Equal(expectedCursor, entity.Cursor);
        }
    }

    public class SetDateModified
    {
        [Fact]
        public void ShouldSetDateModified_AndIsDirtyFlag()
        {
            var timestamp = new DateTime(2025, 5, 12, 12, 56, 2, DateTimeKind.Utc);
            var entity = new ExampleEntity();
            Assert.NotEqual(timestamp, entity.DateModified);
            Assert.False(entity.IsDirty);

            using var dtFix = new DateTimeOffsetProviderContext(timestamp);
            entity.CallSetModified();
            Assert.Equal(timestamp, entity.DateModified);
            Assert.True(entity.IsDirty);
        }
    }

    public class SoftDelete
    {
        [Fact]
        public void ShouldSetDateDeleted()
        {
            var timestamp = new DateTime(2025, 5, 12, 12, 53, 1, DateTimeKind.Utc);
            using var _ = new DateTimeOffsetProviderContext(timestamp);

            var entity = new ExampleEntity();
            entity.Delete();
            Assert.Equal(timestamp, entity.DateDeleted);
        }
    }

    public class ToStringOverride
    {
        [Fact]
        public void ShouldSetDateDeleted()
        {
            var entity = new ExampleEntity();
            var expected = entity.AsSerializable().Serialize();
            Assert.Equivalent(expected, entity.ToString());
        }
    }

    public class UpdateProperty
    {
        [Fact]
        public void Should_ThrowEntityUpdateException_WhenPropertyNotFound()
        {
            var sut = new ExampleEntity();
            Assert.Throws<EntityUpdateException>(() => sut.UpdateProperty("missing", "value"));
        }

        [Fact]
        public void Should_ThrowEntityUpdateException_WhenTypeMismatch()
        {
            var sut = new ExampleEntity("initial value");
            Assert.Throws<EntityUpdateException>(() => sut.UpdateProperty(nameof(ExampleEntity.Name), 17));
        }

        [Fact]
        public void Should_NotUpdateProperty_WhenValueNull()
        {
            const string initial = "initial-value";
            var sut = new ExampleEntity(initial);
            var result = sut.UpdateProperty(nameof(ExampleEntity.Name), null);
            Assert.False(result);
            Assert.False(sut.IsDirty);
            Assert.Equal(initial, sut.Name);
        }

        [Fact]
        public void Should_NotUpdateProperty_WhenValueUnchanged()
        {
            const string initial = "new value";
            var sut = new ExampleEntity(initial);
            var result = sut.UpdateProperty(nameof(ExampleEntity.Name), initial);
            Assert.False(result);
            Assert.False(sut.IsDirty);
            Assert.Equal(initial, sut.Name);
        }

        [Fact]
        public void Should_UpdateProperty_WhenValueChanged()
        {
            const string newValue = "new value";
            var sut = new ExampleEntity("initial value");
            var result = sut.UpdateProperty(nameof(ExampleEntity.Name), newValue);
            Assert.True(result);
            Assert.True(sut.IsDirty);
            Assert.Equal(newValue, sut.Name);
        }
    }

    private sealed class ExampleEntity(string name = "initial") : EntityBase
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - we need a setter (albeit private) for
        // UpdateProperty to work.
        public string Name { get; private set; } = name;

        public void CallSetModified() => SetDateModified();

        public void Delete() => SoftDelete();

        public override object AsSerializable() => new { Property = "Value" };
    }
}