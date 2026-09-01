using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Employees.ChangeRate;
using Timesheet.Domain;
using Timesheet.Domain.Employees;

namespace Timesheet.Application.Tests.Employees;

public sealed class ChangeEmployeeRateCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();

    private readonly ChangeEmployeeRateCommandHandler _handler;

    public ChangeEmployeeRateCommandHandlerTests()
    {
        _handler = new ChangeEmployeeRateCommandHandler(_employeeRepository);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ThrowsBusinessException()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: "emp-001",
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: 1800m);

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.EmployeeNotFound);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChangesRate()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: "emp-001",
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: 1800m);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 1
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        _employeeRepository.ChangeRateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 4, 1),
            1800m,
            Arg.Any<CancellationToken>())
            .Returns(2L);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.NewRateRevision.Should().Be(2);

        await _employeeRepository.Received(1).ChangeRateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 4, 1),
            1800m,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewRevision()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: "emp-001",
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: 1800m);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 4
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        _employeeRepository.ChangeRateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 4, 1),
            1800m,
            Arg.Any<CancellationToken>())
            .Returns(5L);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.NewRateRevision.Should().Be(5);
    }
}
