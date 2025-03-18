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
    public class UpdateTaskCommandHandlerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly UpdateTaskCommandHandler _handler;

        public UpdateTaskCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TaskManagementTestDb_" + Guid.NewGuid())  // Ensure unique DB name for each test run
                .Options;

            _context = new ApplicationDbContext(options);

            var redisMock = new Mock<IConnectionMultiplexer>();
            var dbMock = new Mock<IDatabase>();
            redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

            _handler = new UpdateTaskCommandHandler(_context, redisMock.Object); // Pass mock to the handler

            _context.Tasks.Add(new TaskEntity
            {
                Title = "Existing Task",
                Description = "Existing Description",
                Status = TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(2)
            });

            _context.SaveChanges();  
        }

        [Fact]
        public async Task Handle_ShouldUpdateTask_WhenTaskExists()
        {
            var command = new UpdateTaskCommand(
                1,
                "Updated Task Title",
                "Updated Description",
                TaskStatus.InProgress.ToString(),
                DateTime.UtcNow.AddDays(5)
            );

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Title.Should().Be("Updated Task Title");
            result.Description.Should().Be("Updated Description");
            result.Status.Should().Be(TaskStatus.InProgress);
            result.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(5), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            var command = new UpdateTaskCommand(
                99,
                "Task Not Found",
                "Description",
                TaskStatus.Completed.ToString(),
                DateTime.UtcNow.AddDays(1)
            );

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}