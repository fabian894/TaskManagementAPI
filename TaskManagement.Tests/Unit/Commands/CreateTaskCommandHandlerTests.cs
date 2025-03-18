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
using Xunit;

namespace TaskManagement.Tests.Commands
{
    public class CreateTaskCommandHandlerTests
    {
        private readonly CreateTaskCommandHandler _handler;
        private readonly ApplicationDbContext _dbContext;

        public CreateTaskCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TaskDb_Test")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            var redisMock = new Mock<IConnectionMultiplexer>();
            var dbMock = new Mock<IDatabase>();
            redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

            _handler = new CreateTaskCommandHandler(_dbContext, redisMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateTask_WhenValidRequestIsGiven()
        {
            var command = new CreateTaskCommand(
                "Test Task",
                "Test Description",
                "Pending",
                DateTime.UtcNow.AddDays(3)
            );

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("Test Task");

            var taskInDb = await _dbContext.Tasks.FindAsync(result.Id);
            taskInDb.Should().NotBeNull();
        }
    }
}
