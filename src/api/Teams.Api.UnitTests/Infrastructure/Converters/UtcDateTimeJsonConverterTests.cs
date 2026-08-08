using System.Text.Json;
using Teams.Api.Infrastructure.Converters;

namespace Teams.Api.UnitTests.Infrastructure.Converters;

public static class UtcDateTimeJsonConverterTests
{
    private record TestModel(DateTime Value);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UtcDateTimeJsonConverter());
        return options;
    }

    public class Read
    {
        [Fact]
        public void ReturnsValueUnchanged_WhenValueHasZOffset()
        {
            const string json = """{"Value":"2026-01-01T10:00:00Z"}""";

            var result = JsonSerializer.Deserialize<TestModel>(json, CreateOptions());

            Assert.NotNull(result);
            Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
            Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), result.Value);
        }

        [Fact]
        public void ConvertsToUtc_WhenValueHasNonZeroOffset()
        {
            // 2026-01-01T12:00:00+02:00 is the same instant as 2026-01-01T10:00:00Z.
            const string json = """{"Value":"2026-01-01T12:00:00+02:00"}""";

            var result = JsonSerializer.Deserialize<TestModel>(json, CreateOptions());

            Assert.NotNull(result);
            Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
            Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), result.Value);
        }

        [Fact]
        public void TreatsAsUtc_WhenValueHasNoOffset()
        {
            const string json = """{"Value":"2026-01-01T10:00:00"}""";

            var result = JsonSerializer.Deserialize<TestModel>(json, CreateOptions());

            Assert.NotNull(result);
            Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
            Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), result.Value);
        }
    }

    public class Write
    {
        [Fact]
        public void WritesValueUnchanged_WhenAlreadyUtc()
        {
            var expected = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var model = new TestModel(expected);

            var json = JsonSerializer.Serialize(model, CreateOptions());
            var roundTripped = JsonSerializer.Deserialize<TestModel>(json); // default converter - no offset stripping

            Assert.NotNull(roundTripped);
            Assert.Equal(expected, roundTripped.Value);
            Assert.Equal(DateTimeKind.Utc, roundTripped.Value.Kind);
        }

        [Fact]
        public void ConvertsToUtc_WhenValueIsLocal()
        {
            var utcInstant = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var model = new TestModel(utcInstant.ToLocalTime()); // same instant, Kind = Local

            var json = JsonSerializer.Serialize(model, CreateOptions());
            var roundTripped = JsonSerializer.Deserialize<TestModel>(json);

            Assert.NotNull(roundTripped);
            Assert.Equal(utcInstant, roundTripped.Value);
            Assert.Equal(DateTimeKind.Utc, roundTripped.Value.Kind);
        }

        [Fact]
        public void TreatsAsUtc_WhenValueIsUnspecified()
        {
            var unspecified = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);
            var model = new TestModel(unspecified);

            var json = JsonSerializer.Serialize(model, CreateOptions());
            var roundTripped = JsonSerializer.Deserialize<TestModel>(json);

            Assert.NotNull(roundTripped);
            Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), roundTripped.Value);
            Assert.Equal(DateTimeKind.Utc, roundTripped.Value.Kind);
        }
    }
}