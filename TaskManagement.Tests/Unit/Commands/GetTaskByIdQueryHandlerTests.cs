using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using TaskManagementAPI.CQRS.Queries;
using TaskManagementAPI.CQRS.Queries.Handlers;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace TaskManagement.Tests.Queries
{
    public class GetTaskByIdQueryHandlerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GetTaskByIdQueryHandler _handler;
        private readonly Mock<IConnectionMultiplexer> _redisMock;

        public GetTaskByIdQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _redisMock = new Mock<IConnectionMultiplexer>();
            var dbMock = new Mock<IDatabase>();

            dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync((RedisValue)RedisValue.Null);

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

            // Mock ILogger<GetTaskByIdQueryHandler>
            var loggerMock = new Mock<ILogger<GetTaskByIdQueryHandler>>();

            _handler = new GetTaskByIdQueryHandler(_context, _redisMock.Object, loggerMock.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Tasks.AddRange(new[]
            {
                new TaskEntity { Id = 1, Title = "Task 1", Status = TaskStatus.Pending, DueDate = DateTime.UtcNow },
                new TaskEntity { Id = 2, Title = "Task 2", Status = TaskStatus.Completed, DueDate = DateTime.UtcNow.AddDays(1) }
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task Handle_ShouldReturnTask_WhenTaskExists()
        {
            var query = new GetTaskByIdQuery(1);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Title.Should().Be("Task 1");
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
}
