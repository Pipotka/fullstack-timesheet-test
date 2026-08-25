using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Timesheet.Infrastructure.MongoDb.DocumentMapping;
using Timesheet.Infrastructure.MongoDb.Documents;
using Timesheet.Infrastructure.MongoDb.Mappings;

namespace Timesheet.Infrastructure.Tests.MongoDb.Mappings;

public sealed class BsonMappingTests
{
    [Fact]
    public void DateOnlySerializer_RoundTrip_PreservesValue()
    {
        var serializer = new DateOnlySerializer();
        var original = new DateOnly(2026, 8, 25);

        var bson = SerializeToBson(serializer, original);
        var deserialized = DeserializeFromBson(serializer, bson);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void DateOnlySerializer_SerializesAsString_InYyyyMmDdFormat()
    {
        var serializer = new DateOnlySerializer();
        var date = new DateOnly(2026, 8, 25);

        var bsonValue = SerializeToBsonValue(serializer, date);

        bsonValue.BsonType.Should().Be(BsonType.String);
        bsonValue.AsString.Should().Be("2026-08-25");
    }

    [Fact]
    public void DateOnlySerializer_Deserialize_ThrowsOnInvalidFormat()
    {
        var serializer = new DateOnlySerializer();
        var invalidBson = BsonValue.Create("25-08-2026");

        var act = () => DeserializeFromBson(serializer, invalidBson);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void DateOnlySerializer_RoundTrip_HandlesLeapYear()
    {
        var serializer = new DateOnlySerializer();
        var original = new DateOnly(2024, 2, 29);

        var bson = SerializeToBsonValue(serializer, original);
        var deserialized = DeserializeFromBson(serializer, bson);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void DateOnlySerializer_SerializesAsString_WithLeadingZeros()
    {
        var serializer = new DateOnlySerializer();
        var date = new DateOnly(2026, 1, 5);

        var bsonValue = SerializeToBsonValue(serializer, date);

        bsonValue.BsonType.Should().Be(BsonType.String);
        bsonValue.AsString.Should().Be("2026-01-05");
    }

    private static BsonValue SerializeToBsonValue<T>(IBsonSerializer<T> serializer, T value)
    {
        var doc = new BsonDocument();
        using (var writer = new BsonDocumentWriter(doc))
        {
            writer.WriteStartDocument();
            writer.WriteName("value");
            var context = BsonSerializationContext.CreateRoot(writer);
            serializer.Serialize(context, new BsonSerializationArgs(), value);
            writer.WriteEndDocument();
        }
        return doc["value"];
    }

    private static BsonValue SerializeToBson<T>(IBsonSerializer<T> serializer, T value)
    {
        return SerializeToBsonValue(serializer, value);
    }

    private static T DeserializeFromBson<T>(IBsonSerializer<T> serializer, BsonValue bson)
    {
        var doc = new BsonDocument("value", bson);
        using var reader = new BsonDocumentReader(doc);
        reader.ReadStartDocument();
        reader.ReadName();
        var context = BsonDeserializationContext.CreateRoot(reader);
        var result = serializer.Deserialize(context, new BsonDeserializationArgs());
        reader.ReadEndDocument();
        return result ?? throw new InvalidOperationException("Deserialization returned null");
    }
}

public sealed class BsonClassMapConfiguratorTests
{
    [Fact]
    public void Configure_IsIdempotent()
    {
        var act = () =>
        {
            BsonClassMapConfigurator.Configure();
            BsonClassMapConfigurator.Configure();
            BsonClassMapConfigurator.Configure();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Configure_TimeEntryDocument_RoundTrip_PreservesAllValues()
    {
        BsonClassMapConfigurator.Configure();

        var original = new TimeEntryDocument
        {
            Id = "te-001",
            EmployeeId = "emp-001",
            ProjectId = "proj-001",
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.5m,
            Comment = "Test work",
            AppliedRate = 150.75m,
            Cost = 1281.38m,
            RateRevision = 5,
            Version = 3
        };

        var bson = SerializeToBson(original);
        var restored = DeserializeFromBson<TimeEntryDocument>(bson);

        restored.Id.Should().Be(original.Id);
        restored.EmployeeId.Should().Be(original.EmployeeId);
        restored.ProjectId.Should().Be(original.ProjectId);
        restored.Date.Should().Be(original.Date);
        restored.Hours.Should().Be(original.Hours);
        restored.Comment.Should().Be(original.Comment);
        restored.AppliedRate.Should().Be(original.AppliedRate);
        restored.Cost.Should().Be(original.Cost);
        restored.RateRevision.Should().Be(original.RateRevision);
        restored.Version.Should().Be(original.Version);
    }

    [Fact]
    public void Configure_EmployeeDocument_RoundTrip_PreservesAllValues()
    {
        BsonClassMapConfigurator.Configure();

        var original = new EmployeeDocument
        {
            Id = "emp-001",
            FullName = "John Doe",
            RateHistory =
            [
                new RateHistoryEntryDocument { From = new DateOnly(2024, 1, 1), Rate = 100.50m },
                new RateHistoryEntryDocument { From = new DateOnly(2025, 1, 1), Rate = 150.75m }
            ],
            RateRevision = 42
        };

        var bson = SerializeToBson(original);
        var restored = DeserializeFromBson<EmployeeDocument>(bson);

        restored.Id.Should().Be(original.Id);
        restored.FullName.Should().Be(original.FullName);
        restored.RateRevision.Should().Be(original.RateRevision);
        restored.RateHistory.Should().HaveCount(2);
        restored.RateHistory[0].From.Should().Be(original.RateHistory[0].From);
        restored.RateHistory[0].Rate.Should().Be(original.RateHistory[0].Rate);
        restored.RateHistory[1].From.Should().Be(original.RateHistory[1].From);
        restored.RateHistory[1].Rate.Should().Be(original.RateHistory[1].Rate);
    }

    [Fact]
    public void Configure_ProjectDocument_RoundTrip_PreservesAllValues_WithEndDate()
    {
        BsonClassMapConfigurator.Configure();

        var original = new ProjectDocument
        {
            Id = "proj-001",
            Code = "PRJ001",
            Name = "Test Project",
            Budget = 50000.75m,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31)
        };

        var bson = SerializeToBson(original);
        var restored = DeserializeFromBson<ProjectDocument>(bson);

        restored.Id.Should().Be(original.Id);
        restored.Code.Should().Be(original.Code);
        restored.Name.Should().Be(original.Name);
        restored.Budget.Should().Be(original.Budget);
        restored.StartDate.Should().Be(original.StartDate);
        restored.EndDate.Should().Be(original.EndDate);
    }

    [Fact]
    public void Configure_ProjectDocument_RoundTrip_PreservesNullableEndDate()
    {
        BsonClassMapConfigurator.Configure();

        var original = new ProjectDocument
        {
            Id = "proj-002",
            Code = "PRJ002",
            Name = "Ongoing Project",
            Budget = 100000.00m,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = null
        };

        var bson = SerializeToBson(original);
        var restored = DeserializeFromBson<ProjectDocument>(bson);

        restored.Id.Should().Be(original.Id);
        restored.Code.Should().Be(original.Code);
        restored.Name.Should().Be(original.Name);
        restored.Budget.Should().Be(original.Budget);
        restored.StartDate.Should().Be(original.StartDate);
        restored.EndDate.Should().BeNull();
    }

    [Fact]
    public void Configure_PeriodClosureDocument_RoundTrip_PreservesAllValues()
    {
        BsonClassMapConfigurator.Configure();

        var original = new PeriodClosureDocument
        {
            Year = 2024,
            Month = 8,
            IsClosed = true
        };

        var bson = SerializeToBson(original);
        var restored = DeserializeFromBson<PeriodClosureDocument>(bson);

        restored.Year.Should().Be(original.Year);
        restored.Month.Should().Be(original.Month);
        restored.IsClosed.Should().Be(original.IsClosed);
    }

    [Fact]
    public void Configure_TimeEntryDocument_DecimalFields_AreDecimal128()
    {
        BsonClassMapConfigurator.Configure();

        var doc = new TimeEntryDocument
        {
            Id = "te-001",
            EmployeeId = "emp-001",
            ProjectId = "proj-001",
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.5m,
            Comment = "Test",
            AppliedRate = 150.75m,
            Cost = 1281.38m,
            RateRevision = 5,
            Version = 3
        };

        var bsonDoc = SerializeToBsonDocument(doc);

        bsonDoc["hours"].BsonType.Should().Be(BsonType.Decimal128);
        bsonDoc["appliedRate"].BsonType.Should().Be(BsonType.Decimal128);
        bsonDoc["cost"].BsonType.Should().Be(BsonType.Decimal128);
    }

    [Fact]
    public void Configure_TimeEntryDocument_DateField_IsString()
    {
        BsonClassMapConfigurator.Configure();

        var doc = new TimeEntryDocument
        {
            Id = "te-001",
            EmployeeId = "emp-001",
            ProjectId = "proj-001",
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.0m,
            Comment = "Test",
            AppliedRate = 100.00m,
            Cost = 800.00m,
            RateRevision = 1,
            Version = 1
        };

        var bsonDoc = SerializeToBsonDocument(doc);

        bsonDoc["date"].BsonType.Should().Be(BsonType.String);
        bsonDoc["date"].AsString.Should().Be("2024-08-25");
    }

    [Fact]
    public void Configure_ProjectDocument_BudgetField_IsDecimal128()
    {
        BsonClassMapConfigurator.Configure();

        var doc = new ProjectDocument
        {
            Id = "proj-001",
            Code = "PRJ001",
            Name = "Test",
            Budget = 50000.75m,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = null
        };

        var bsonDoc = SerializeToBsonDocument(doc);

        bsonDoc["budget"].BsonType.Should().Be(BsonType.Decimal128);
    }

    private static BsonDocument SerializeToBsonDocument<T>(T value)
    {
        return value.ToBsonDocument();
    }

    private static byte[] SerializeToBson<T>(T value)
    {
        return value.ToBson();
    }

    private static T DeserializeFromBson<T>(byte[] bson)
    {
        return BsonSerializer.Deserialize<T>(bson);
    }
}
