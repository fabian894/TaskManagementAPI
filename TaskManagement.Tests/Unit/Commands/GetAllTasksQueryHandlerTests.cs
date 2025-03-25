using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetAllTasksQueryHandlerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GetAllTasksQueryHandler _handler;
        private readonly Mock<IConnectionMultiplexer> _redisMock;

        public GetAllTasksQueryHandlerTests()
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

            // Mocking ILogger<GetAllTasksQueryHandler>
            var loggerMock = new Mock<ILogger<GetAllTasksQueryHandler>>();

            _handler = new GetAllTasksQueryHandler(_context, _redisMock.Object, loggerMock.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Tasks.AddRange(new List<TaskEntity>
            {
                new TaskEntity { Id = 1, Title = "Task 1", Status = TaskStatus.Pending, DueDate = DateTime.UtcNow },
                new TaskEntity { Id = 2, Title = "Task 2", Status = TaskStatus.Completed, DueDate = DateTime.UtcNow.AddDays(1) }
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task Handle_ShouldReturnAllTasks_WhenTasksExist()
        {
            var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
            result.First().Title.Should().Be("Task 1");
            result.Last().Title.Should().Be("Task 2");
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
            _context.Dispose();
        }
    }
}
