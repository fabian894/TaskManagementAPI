using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.CQRS.Queries.Handlers;
using TaskManagementAPI.CQRS.Queries;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using StackExchange.Redis;

public class GetTaskByIdQueryHandlerIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetTaskByIdQueryHandler _handler;
    private readonly Mock<IConnectionMultiplexer> _redisMock;

    public GetTaskByIdQueryHandlerIntegrationTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"))
            .BuildServiceProvider();

        _context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _redisMock = new Mock<IConnectionMultiplexer>();
        var dbMock = new Mock<IDatabase>();

        dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync((RedisValue)RedisValue.Null);

        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        _handler = new GetTaskByIdQueryHandler(_context, _redisMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTask_WhenTaskExists()
    {
        _context.Tasks.Add(new TaskEntity { Id = 1, Title = "Test Task", Status = TaskStatus.Pending, DueDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var query = new GetTaskByIdQuery(1);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
