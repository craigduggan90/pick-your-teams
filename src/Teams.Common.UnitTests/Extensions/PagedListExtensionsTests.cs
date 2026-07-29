using Teams.Common.Extensions;
using Teams.Common.Pagination;
using System.Globalization;

namespace Teams.Common.UnitTests.Extensions;

public static class PagedListExtensionsTests
{
    public class Map
    {
        [Fact]
        public void ProjectsEachItem_UsingConverter()
        {
            var input = new PagedList<int>([1, 2, 3], "cursor-abc", 3);

            var result = input.Map(value => value.ToString(CultureInfo.InvariantCulture));

            Assert.Equal(["1", "2", "3"], result.Data);
        }

        [Fact]
        public void PreservesCursor_WhenMapped()
        {
            var input = new PagedList<int>([1, 2, 3], "cursor-abc", 3);

            var result = input.Map(value => value.ToString(CultureInfo.InvariantCulture));

            Assert.Equal("cursor-abc", result.Cursor);
        }

        [Fact]
        public void PreservesCount_WhenMapped()
        {
            var input = new PagedList<int>([1, 2, 3], "cursor-abc", 3);

            var result = input.Map(value => value.ToString(CultureInfo.InvariantCulture));

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void PreservesNullCursor_WhenMapped()
        {
            var input = new PagedList<int>([1, 2, 3], null, 3);

            var result = input.Map(value => value.ToString(CultureInfo.InvariantCulture));

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void ReturnsEmptyData_WhenInputDataIsEmpty()
        {
            var input = new PagedList<int>([], null, 0);

            var result = input.Map(value => value.ToString(CultureInfo.InvariantCulture));

            Assert.Empty(result.Data);
        }

        [Fact]
        public void DoesNotMutateOriginalInput()
        {
            var input = new PagedList<int>([1, 2, 3], "cursor-abc", 3);

            _ = input.Map(value => value * 10);

            Assert.Equal([1, 2, 3], input.Data);
        }
    }
}