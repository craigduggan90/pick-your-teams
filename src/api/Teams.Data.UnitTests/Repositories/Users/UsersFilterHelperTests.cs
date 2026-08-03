using Teams.Data.Models;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Data.UnitTests.Repositories.Users;

public static class UsersFilterHelperTests
{
    private static IQueryable<User> GetSeedData(int count) =>
        Enumerable.Range(1, count).Select(SeedDataFactory.Users.Create).AsQueryable();

    public class ApplyEmailAddressFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyEmailAddressFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var value = SeedDataFactory.Users.GetIdentifier(15);
            var data = GetSeedData(30);
            var expected = data.Where(user => user.EmailAddress.Contains(value));
            var filtered = data.ApplyEmailAddressFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyTagFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyTagFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const string value = "tag-00000015";
            var data = GetSeedData(30);
            var expected = data.Where(user => user.Tag.Contains(value));
            var filtered = data.ApplyTagFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyDisplayNameFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyDisplayNameFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const string value = "00000015";
            var data = GetSeedData(30);
            var expected = data.Where(user => user.DisplayName.Contains(value));
            var filtered = data.ApplyDisplayNameFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyRatingFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyRatingFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFromFilter_WhenOnlyFromProvided()
        {
            // User at index 2 has a rating of exactly 1020 - using that as the boundary confirms
            // >= is inclusive of an exact match, not just anyone strictly above it.
            var data = GetSeedData(30);
            var expected = data.Where(user => user.Rating >= 1020);
            var filtered = data.ApplyRatingFilter(new RangeFilter<int>(1020, null));
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldApplyToFilter_WhenOnlyToProvided()
        {
            // Same boundary (1020) from the other side - confirms < excludes an exact match rather
            // than including it.
            var data = GetSeedData(30);
            var expected = data.Where(user => user.Rating < 1020);
            var filtered = data.ApplyRatingFilter(new RangeFilter<int>(null, 1020));
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldApplyBothFilters_WhenFromAndToProvided()
        {
            var data = GetSeedData(30);
            var expected = data.Where(user => user.Rating >= 900 && user.Rating < 1100);
            var filtered = data.ApplyRatingFilter(new RangeFilter<int>(900, 1100));
            Assert.Equivalent(expected, filtered, true);
        }
    }
}