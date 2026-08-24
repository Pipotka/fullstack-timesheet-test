using FluentAssertions;
using FluentValidation;
using Timesheet.Application.Reports.ProjectReport;

namespace Timesheet.Application.Tests.Reports;

public sealed class ProjectReportQueryValidatorTests
{
    private readonly IValidator<ProjectReportQuery> _validator = new ProjectReportQueryValidator();

    [Fact]
    public void Validate_InvalidMonth_HasError()
    {
        var query = new ProjectReportQuery(Year: 2026, Month: 0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Month");
    }

    [Fact]
    public void Validate_MonthAbove12_HasError()
    {
        var query = new ProjectReportQuery(Year: 2026, Month: 13);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Month");
    }

    [Fact]
    public void Validate_InvalidYear_HasError()
    {
        var query = new ProjectReportQuery(Year: 0, Month: 8);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Year");
    }

    [Fact]
    public void Validate_ValidQuery_Passes()
    {
        var query = new ProjectReportQuery(Year: 2026, Month: 8);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
