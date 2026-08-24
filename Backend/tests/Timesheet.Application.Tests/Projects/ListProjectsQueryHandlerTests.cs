using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Projects.List;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Domain.Projects;

namespace Timesheet.Application.Tests.Projects;

public sealed class ListProjectsQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();

    private readonly ListProjectsQueryHandler _handler;

    public ListProjectsQueryHandlerTests()
    {
        _handler = new ListProjectsQueryHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_ReturnsAllProjects()
    {
        var projects = new List<Project>
        {
            new()
            {
                Id = new ProjectId("proj-001"),
                Code = "PRJ-001",
                Name = "Project 1",
                Budget = 1000000,
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31)
            },
            new()
            {
                Id = new ProjectId("proj-002"),
                Code = "PRJ-002",
                Name = "Project 2",
                Budget = 2000000,
                StartDate = null,
                EndDate = null
            }
        };

        _projectRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(projects);

        var result = await _handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_MapsFields()
    {
        var projects = new List<Project>
        {
            new()
            {
                Id = new ProjectId("proj-001"),
                Code = "PRJ-001",
                Name = "Project 1",
                Budget = 1500000.50m,
                StartDate = new DateOnly(2026, 3, 1),
                EndDate = new DateOnly(2026, 9, 30)
            }
        };

        _projectRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(projects);

        var result = await _handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        result[0].Id.Should().Be("proj-001");
        result[0].Code.Should().Be("PRJ-001");
        result[0].Name.Should().Be("Project 1");
        result[0].Budget.Should().Be(1500000.50m);
        result[0].StartDate.Should().Be(new DateOnly(2026, 3, 1));
        result[0].EndDate.Should().Be(new DateOnly(2026, 9, 30));
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        _projectRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project>());

        var result = await _handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
