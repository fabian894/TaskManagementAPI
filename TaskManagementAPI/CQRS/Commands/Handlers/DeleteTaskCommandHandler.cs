using MediatR;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;
        private readonly ILogger<DeleteTaskCommandHandler> _logger;

        public DeleteTaskCommandHandler(ApplicationDbContext context, IConnectionMultiplexer redis, ILogger<DeleteTaskCommandHandler> logger)
        {
            _context = context;
            _cache = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found.", request.Id);
                return false;
            }

            _logger.LogInformation("Attempting to delete Task ID {TaskId}: Title - {Title}, Status - {Status}",
                task.Id, task.Title, task.Status);

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task ID {TaskId} deleted successfully.", task.Id);

            // Clear the cache
            await _cache.KeyDeleteAsync($"task_{request.Id}");
            await _cache.KeyDeleteAsync("all_tasks");

            return true;
        }
    }
}
