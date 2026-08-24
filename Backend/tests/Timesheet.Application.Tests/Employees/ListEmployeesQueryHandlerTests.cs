using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Employees.List;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Domain.Employees;

namespace Timesheet.Application.Tests.Employees;

public sealed class ListEmployeesQueryHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();

    private readonly ListEmployeesQueryHandler _handler;

    public ListEmployeesQueryHandlerTests()
    {
        _handler = new ListEmployeesQueryHandler(_employeeRepository);
    }

    [Fact]
    public async Task Handle_ReturnsAllEmployees()
    {
        var employees = new List<Employee>
        {
            new()
            {
                Id = new EmployeeId("emp-001"),
                FullName = "Employee 1",
                RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1000 }],
                RateRevision = 1
            },
            new()
            {
                Id = new EmployeeId("emp-002"),
                FullName = "Employee 2",
                RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 2000 }],
                RateRevision = 1
            }
        };

        _employeeRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(employees);

        var result = await _handler.Handle(new ListEmployeesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_MapsCurrentRate_FromLastRateHistoryEntry()
    {
        var employees = new List<Employee>
        {
            new()
            {
                Id = new EmployeeId("emp-001"),
                FullName = "Employee 1",
                RateHistory =
                [
                    new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1000 },
                    new RateHistoryEntry { From = new DateOnly(2026, 4, 1), Rate = 1500 }
                ],
                RateRevision = 2
            }
        };

        _employeeRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(employees);

        var result = await _handler.Handle(new ListEmployeesQuery(), CancellationToken.None);

        result[0].CurrentRate.Should().Be(1500m);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        _employeeRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Employee>());

        var result = await _handler.Handle(new ListEmployeesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
