using Serilog;
using MediatR;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskEntity>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;
        private readonly ILogger<CreateTaskCommandHandler> _logger;

        public CreateTaskCommandHandler(ApplicationDbContext context, IConnectionMultiplexer redis, ILogger<CreateTaskCommandHandler> logger)
        {
            _context = context;
            _cache = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<TaskEntity> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CreateTaskCommand: {Title}, {Status}", request.Title, request.Status);

            var task = new TaskEntity
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate
            };

            if (request.Status == "InProgress")
            {
                task.MoveTo(TaskTrigger.Start);
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task created successfully with ID {TaskId} and Status {TaskStatus}", task.Id, task.Status);

            await _cache.KeyDeleteAsync("all_tasks");

            return task;
        }
    }
}
