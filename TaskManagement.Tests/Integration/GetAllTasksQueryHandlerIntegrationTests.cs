using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.CQRS.Queries;
using TaskManagementAPI.CQRS.Queries.Handlers;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Xunit;

namespace TaskManagement.Tests.Integration
{
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

            // Ensure a fresh database
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _handler = new GetTaskByIdQueryHandler(_context);
        }

        private async Task<int> SeedTestData()
        {
            _context.Tasks.RemoveRange(_context.Tasks);
            await _context.SaveChangesAsync();

            var task = new TaskEntity
            {
                Title = "Sample Task",
                Status = TaskStatus.Pending, 
                DueDate = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return task.Id; 
        }


        [Fact]
        public async Task Handle_ShouldReturnTask_WhenTaskExists()
        {
            int taskId = await SeedTestData();

            var result = await _handler.Handle(new GetTaskByIdQuery(taskId), CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(taskId);
            result.Title.Should().Be("Sample Task");
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            _context.Tasks.RemoveRange(_context.Tasks);
            await _context.SaveChangesAsync();

            int nonExistentId = 999;

            var result = await _handler.Handle(new GetTaskByIdQuery(nonExistentId), CancellationToken.None);

            result.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
