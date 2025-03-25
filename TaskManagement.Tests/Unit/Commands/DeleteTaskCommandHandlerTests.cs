using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.CQRS.Handlers;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace TaskManagement.Tests.Commands
{
    public class DeleteTaskCommandHandlerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly DeleteTaskCommandHandler _handler;

        public DeleteTaskCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TaskDb_" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            var redisMock = new Mock<IConnectionMultiplexer>();
            var dbMock = new Mock<IDatabase>();
            redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

            // Mocking ILogger<DeleteTaskCommandHandler>
            var loggerMock = new Mock<ILogger<DeleteTaskCommandHandler>>();

            SeedDatabase();

            _handler = new DeleteTaskCommandHandler(_context, redisMock.Object, loggerMock.Object);
        }

        private void SeedDatabase()
        {
            _context.Tasks.Add(new TaskEntity
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(2)
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task Handle_ShouldDeleteTask_WhenTaskExists()
        {
            var command = new DeleteTaskCommand(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            var task = await _context.Tasks.FindAsync(1);
            task.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            var command = new DeleteTaskCommand(99);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
