using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using TaskManagementAPI.CQRS.Queries;
using TaskManagementAPI.CQRS.Queries.Handlers;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Xunit;

namespace TaskManagement.Tests.Integration
{
    public class GetAllTasksQueryHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GetAllTasksQueryHandler _handler;
        private readonly Mock<IConnectionMultiplexer> _redisMock;

        public GetAllTasksQueryHandlerIntegrationTests()
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

            _handler = new GetAllTasksQueryHandler(_context, _redisMock.Object);
        }

        private async Task SeedTestData()
        {
            _context.Tasks.RemoveRange(_context.Tasks);
            await _context.SaveChangesAsync();

            _context.Tasks.AddRange(
                new TaskEntity { Title = "Task 1", Status = TaskStatus.Pending, DueDate = DateTime.UtcNow },
                new TaskEntity { Title = "Task 2", Status = TaskStatus.Completed, DueDate = DateTime.UtcNow }
            );

            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_ShouldReturnAllTasks_WhenTasksExist()
        {
            await SeedTestData();

            var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoTasksExist()
        {
            _context.Tasks.RemoveRange(_context.Tasks);
            await _context.SaveChangesAsync();

            var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
