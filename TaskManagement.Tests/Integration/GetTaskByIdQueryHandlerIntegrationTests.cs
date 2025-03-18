using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.CQRS.Queries.Handlers;
using TaskManagementAPI.CQRS.Queries;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

public class GetTaskByIdQueryHandlerIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetTaskByIdQueryHandler _handler;

    public GetTaskByIdQueryHandlerIntegrationTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"))
            .BuildServiceProvider();

        _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        _context.Database.EnsureCreated();
        _handler = new GetTaskByIdQueryHandler(_context);

        SeedTestData().GetAwaiter().GetResult();
    }

    private async Task SeedTestData()
    {
        _context.Tasks.AddRange(new[]
        {
        new TaskEntity { Title = "Integration Task 1", Status = TaskStatus.Pending, DueDate = DateTime.UtcNow },
        new TaskEntity { Title = "Integration Task 2", Status = TaskStatus.Completed, DueDate = DateTime.UtcNow.AddDays(1) }
    });

        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task Handle_ShouldReturnTask_WhenTaskExists()
    {
        var query = new GetTaskByIdQuery(1);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Integration Task 1");
        result.Status.Should().Be(TaskStatus.Pending); 
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        var query = new GetTaskByIdQuery(99);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
