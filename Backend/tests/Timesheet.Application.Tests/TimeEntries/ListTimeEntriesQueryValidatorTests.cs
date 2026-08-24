using FluentAssertions;
using Timesheet.Application.TimeEntries.List;

namespace Timesheet.Application.Tests.TimeEntries;

public sealed class ListTimeEntriesQueryValidatorTests
{
    private readonly ListTimeEntriesQueryValidator _validator = new();

    [Fact]
    public void Validate_WithPageSize201_Fails()
    {
        var query = new ListTimeEntriesQuery(PageSize: 201);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ListTimeEntriesQuery.PageSize));
    }

    [Fact]
    public void Validate_WithPage0_Fails()
    {
        var query = new ListTimeEntriesQuery(Page: 0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ListTimeEntriesQuery.Page));
    }

    [Fact]
    public void Validate_WithPageSize0_Fails()
    {
        var query = new ListTimeEntriesQuery(PageSize: 0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ListTimeEntriesQuery.PageSize));
    }

    [Fact]
    public void Validate_WithPageSizeNegative_Fails()
    {
        var query = new ListTimeEntriesQuery(PageSize: -1);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ListTimeEntriesQuery.PageSize));
    }

    [Fact]
    public void Validate_WithPageNegative_Fails()
    {
        var query = new ListTimeEntriesQuery(Page: -1);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ListTimeEntriesQuery.Page));
    }

    [Fact]
    public void Validate_WithPageSize200_Passes()
    {
        var query = new ListTimeEntriesQuery(PageSize: 200);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPageSize1_Passes()
    {
        var query = new ListTimeEntriesQuery(PageSize: 1);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPage1_Passes()
    {
        var query = new ListTimeEntriesQuery(Page: 1);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithDefaults_Passes()
    {
        var query = new ListTimeEntriesQuery();

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithAllValidParameters_Passes()
    {
        var query = new ListTimeEntriesQuery(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            FromDate: new DateOnly(2026, 1, 1),
            ToDate: new DateOnly(2026, 12, 31),
            Page: 2,
            PageSize: 100);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
