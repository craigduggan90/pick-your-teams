using Teams.Data.Filters;
using Teams.Domain.Interfaces;

namespace Teams.Data.UnitTests.Filters;

public static class HasCreatedTimestampQueryFilterTests
{
    private static readonly DateTime BaseDate = new(2020, 1, 1, 12, 0, 0);
    private static readonly DateTime MaxSeedDate = BaseDate.AddDays(99);

    private sealed class HasCreated(DateTime date) : IHasCreatedTimestamp
    {
        public DateTime DateCreated { get; } = date;
    }

    /// <summary>Gets a collection of <see cref="HasCreated"/> representing dates from 2020-01-01 to 2020-04-09 (inclusive)</summary>
    private static IQueryable<HasCreated> SeedData
        => Enumerable.Range(0, 100)
            .Select(i => new HasCreated(BaseDate.AddDays(i)))
            .AsQueryable();

    public class ApplyCreatedFromFilter
    {
        [Fact]
        public void Should_ReturnUnfilteredCollection_WhenValueIsNull()
        {
            DateTime? value = null;
            var collection = SeedData;
            var actual = collection.ApplyCreatedFromFilter(value);
            Assert.Same(collection, actual);
        }

        [Fact]
        public void Should_ReturnFilteredCollection_WhenValueProvided()
        {
            // Our seed data starts at 2020-01-01, this sets the value as 2020-01-31
            DateTime? value = BaseDate.AddDays(30);
            var collection = SeedData;

            // The result should be applied as >= value
            var expected = collection.Where(e => e.DateCreated >= value);
            var actual = collection.ApplyCreatedFromFilter(value);
            Assert.Equivalent(expected, actual, true);

            // The result should return records from 2020-01-31 to max (2020-04-09 12:00:00)
            var dates = actual.Select(a => a.DateCreated).ToList();
            Assert.Equal(MaxSeedDate, dates.Max());
            Assert.Equal(value, dates.Min());
        }
    }

    public class ApplyCreatedToFilter
    {
        [Fact]
        public void Should_ReturnUnfilteredCollection_WhenValueIsNull()
        {
            DateTime? value = null;
            var collection = SeedData;
            var actual = collection.ApplyCreatedToFilter(value);
            Assert.Same(collection, actual);
        }

        [Fact]
        public void Should_ReturnFilteredCollection_WhenValueProvided()
        {
            // Our seed data starts at 2020-01-01, this sets the value as 2020-01-31
            DateTime? value = BaseDate.AddDays(30);
            var collection = SeedData;

            // The result should be applied as < value
            var expected = collection.Where(e => e.DateCreated < value);
            var actual = collection.ApplyCreatedToFilter(value);
            Assert.Equivalent(expected, actual, true);

            // The result should return records from 2020-01-01 to value-1
            var dates = actual.Select(a => a.DateCreated).ToList();
            Assert.True(MaxSeedDate > dates.Max());
            Assert.Equal(BaseDate, dates.Min());
        }
    }
}