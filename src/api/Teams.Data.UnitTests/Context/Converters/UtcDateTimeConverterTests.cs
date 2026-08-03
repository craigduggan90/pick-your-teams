using Teams.Data.Context.Converters;

namespace Teams.Data.UnitTests.Context.Converters;

public static class UtcDateTimeConverterTests
{
    public class ConvertToProvider
    {
        [Fact]
        public void DateTimeConverter_ConvertsValueToPersist_ToUTC()
        {
            // We can't really check the value here, since Local could be... anything, but we can check that the value is
            // marked as a UTC date time.
            var valueToPersist = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var sut = new UtcDateTimeConverter();

            var actual = sut.ConvertToProviderTyped(valueToPersist);
            Assert.Equal(DateTimeKind.Utc, actual.Kind);
        }
    }

    public class ConvertFromProvider
    {
        [Fact]
        public void DateTimeConverter_ConvertsValueToRetrieve_ToUTC()
        {
            // Database values are always unspecified, so check that it gets flagged as UTC on retrieval 
            var valueToRetrieve = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var sut = new UtcDateTimeConverter();

            var actual = sut.ConvertFromProviderTyped(valueToRetrieve);
            Assert.Equal(DateTimeKind.Utc, actual.Kind);
        }
    }
}