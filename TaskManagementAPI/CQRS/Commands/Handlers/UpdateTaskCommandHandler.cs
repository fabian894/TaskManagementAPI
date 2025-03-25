using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskEntity>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;
        private readonly ILogger<UpdateTaskCommandHandler> _logger;

        public UpdateTaskCommandHandler(ApplicationDbContext context, IConnectionMultiplexer redis, ILogger<UpdateTaskCommandHandler> logger)
        {
            _context = context;
            _cache = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<TaskEntity> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UpdateTaskCommand for Task ID {TaskId}", request.Id);

            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found.", request.Id);
                return null;
            }

            _logger.LogInformation("Updating Task ID {TaskId}: Title - {Title}, Description - {Description}, Status - {Status}",
                request.Id, request.Title, request.Description, request.Status);

            // Update Title and Description if provided
            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;

            // Validate and apply status transition
            if (!Enum.TryParse<TaskStatus>(request.Status, out var newStatus))
            {
                _logger.LogError("Invalid status: {Status} provided for Task ID {TaskId}", request.Status, request.Id);
                throw new ArgumentException($"Invalid status: {request.Status}");
            }

            // Allow only valid transitions
            TaskTrigger? trigger = null;

            if (task.Status == TaskStatus.Pending && newStatus == TaskStatus.InProgress)
            {
                trigger = TaskTrigger.Start;
            }
            else if (task.Status == TaskStatus.InProgress && newStatus == TaskStatus.Completed)
            {
                trigger = TaskTrigger.Complete;
            }
            else if (task.Status == newStatus)
            {
                _logger.LogInformation("No status change for Task ID {TaskId}, status remains {Status}.", request.Id, task.Status);
            }
            else
            {
                _logger.LogWarning("Invalid status transition from {OldStatus} to {NewStatus} for Task ID {TaskId}.",
                    task.Status, newStatus, task.Id);
                throw new InvalidOperationException($"Invalid status transition from {task.Status} to {newStatus}.");
            }

            // Apply state change only if a valid trigger exists
            if (trigger.HasValue)
            {
                task.MoveTo(trigger.Value);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task ID {TaskId} updated successfully with Status {TaskStatus}", task.Id, task.Status);

            // Clear cache
            await _cache.KeyDeleteAsync($"task_{request.Id}");
            await _cache.KeyDeleteAsync("all_tasks");

            return task;
        }
    }
}
