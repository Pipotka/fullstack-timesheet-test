using FluentAssertions;

namespace Timesheet.Domain.Tests.Common;

public sealed class BusinessExceptionTests
{
    [Fact]
    public void Constructor_SetsCodeAndMessage()
    {
        var ex = new BusinessException("PERIOD_CLOSED", "Период закрыт");

        ex.Code.Should().Be("PERIOD_CLOSED");
        ex.Message.Should().Be("Период закрыт");
    }

    [Fact]
    public void InheritsFromException()
    {
        var ex = new BusinessException("CODE", "msg");

        ex.Should().BeAssignableTo<Exception>();
    }
}
