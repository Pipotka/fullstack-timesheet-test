using System.Text.Json;
using FluentAssertions;
using Timesheet.Api.Contracts;

namespace Timesheet.Api.Tests.Contracts;

public class JsonContractTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new DateOnlyJsonConverter());
        return options;
    }

    [Fact]
    public void DateOnly_SerializesAs_yyyy_MM_dd()
    {
        var date = new DateOnly(2026, 3, 15);
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(date, options);

        json.Should().Be("\"2026-03-15\"");
    }

    [Fact]
    public void DateOnly_DeserializesFrom_yyyy_MM_dd()
    {
        var json = "\"2026-03-15\"";
        var options = CreateOptions();

        var date = JsonSerializer.Deserialize<DateOnly>(json, options);

        date.Should().Be(new DateOnly(2026, 3, 15));
    }

    [Fact]
    public void DateOnly_RejectsInvalidFormat()
    {
        var json = "\"15/03/2026\"";
        var options = CreateOptions();

        var act = () => JsonSerializer.Deserialize<DateOnly>(json, options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void NullableDateOnly_SerializesNull()
    {
        DateOnly? date = null;
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(date, options);

        json.Should().Be("null");
    }

    [Fact]
    public void Decimal_PreservesPrecision()
    {
        var request = new CreateTimeEntryRequest("emp-1", "proj-1", "2026-01-15", 8.5m, "test");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(request, options);
        var deserialized = JsonSerializer.Deserialize<CreateTimeEntryRequest>(json, options);

        deserialized!.Hours.Should().Be(8.5m);
    }

    [Fact]
    public void CreateTimeEntryRequest_DeserializesCorrectly()
    {
        var json = """{"employeeId":"emp-1","projectId":"proj-1","date":"2026-01-15","hours":8.5,"comment":"test"}""";
        var options = CreateOptions();

        var request = JsonSerializer.Deserialize<CreateTimeEntryRequest>(json, options);

        request.Should().NotBeNull();
        request!.EmployeeId.Should().Be("emp-1");
        request.ProjectId.Should().Be("proj-1");
        request.Date.Should().Be("2026-01-15");
        request.Hours.Should().Be(8.5m);
        request.Comment.Should().Be("test");
    }

    [Fact]
    public void UpdateTimeEntryRequest_DeserializesCorrectly()
    {
        var json = """{"version":5,"hours":4,"comment":"updated"}""";
        var options = CreateOptions();

        var request = JsonSerializer.Deserialize<UpdateTimeEntryRequest>(json, options);

        request.Should().NotBeNull();
        request!.Version.Should().Be(5);
        request.Hours.Should().Be(4);
        request.Comment.Should().Be("updated");
    }

    [Fact]
    public void PeriodRequest_DeserializesCorrectly()
    {
        var json = """{"year":2026,"month":3}""";
        var options = CreateOptions();

        var request = JsonSerializer.Deserialize<PeriodRequest>(json, options);

        request.Should().NotBeNull();
        request!.Year.Should().Be(2026);
        request.Month.Should().Be(3);
    }

    [Fact]
    public void ErrorResponse_SerializesWithCamelCase()
    {
        var error = new ErrorResponse("VALIDATION_ERROR", "Ошибка валидации");
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(error, options);

        json.Should().Contain("\"code\"");
        json.Should().Contain("\"message\"");
    }
}
