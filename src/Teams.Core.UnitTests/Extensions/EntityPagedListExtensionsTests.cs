using Teams.Common.Pagination;
using Teams.Core.Extensions;
using Teams.Domain.Interfaces;
using System.Globalization;

namespace Teams.Core.UnitTests.Extensions;

public static class EntityPagedListExtensionsTests
{
    private sealed class CursorEntity(long cursor) : IHasCursor
    {
        public long Cursor { get; } = cursor;
    }

    public class Create
    {
        [Fact]
        public void ProjectsEachItem_UsingConverter()
        {
            IReadOnlyList<CursorEntity> list = [new(1), new(2), new(3)];

            var result = list.ToPagedList(entity => entity.Cursor.ToString(CultureInfo.InvariantCulture));

            Assert.Equal(["1", "2", "3"], result.Data);
        }

        [Fact]
        public void SetsCount_ToNumberOfItemsInList()
        {
            IReadOnlyList<CursorEntity> list = [new(1), new(2), new(3)];

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void SetsCursor_ToEncodedValueOfHighestCursor()
        {
            IReadOnlyList<CursorEntity> list = [new(1), new(3), new(2)];
            var expectedCursor = CursorConverter.TryEncodeCursor(3L, out var encoded) ? encoded : null;

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Equal(expectedCursor, result.Cursor);
        }

        [Fact]
        public void SetsCursorToHighestValue_RegardlessOfListOrder()
        {
            IReadOnlyList<CursorEntity> list = [new(10), new(5), new(20), new(15)];
            var expectedCursor = CursorConverter.TryEncodeCursor(20L, out var encoded) ? encoded : null;

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Equal(expectedCursor, result.Cursor);
        }

        [Fact]
        public void SetsCursorToNull_WhenListIsEmpty()
        {
            IReadOnlyList<CursorEntity> list = [];

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void SetsCountToZero_WhenListIsEmpty()
        {
            IReadOnlyList<CursorEntity> list = [];

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void ReturnsEmptyData_WhenListIsEmpty()
        {
            IReadOnlyList<CursorEntity> list = [];

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Empty(result.Data);
        }

        [Fact]
        public void SetsCursor_WhenListHasSingleItem()
        {
            IReadOnlyList<CursorEntity> list = [new(42)];
            var expectedCursor = CursorConverter.TryEncodeCursor(42L, out var encoded) ? encoded : null;

            var result = list.ToPagedList(entity => entity.Cursor);

            Assert.Equal(expectedCursor, result.Cursor);
        }
    }
}