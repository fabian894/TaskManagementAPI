using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, List<TaskEntity>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;
        private readonly ILogger<GetAllTasksQueryHandler> _logger;

        public GetAllTasksQueryHandler(ApplicationDbContext context, IConnectionMultiplexer redis, ILogger<GetAllTasksQueryHandler> logger)
        {
            _context = context;
            _cache = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<List<TaskEntity>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all tasks from the database.");

            var tasks = await _context.Tasks.ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                _logger.LogWarning("No tasks found in the database.");
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {TaskCount} tasks from the database.", tasks.Count);
            }

             await _cache.StringSetAsync("all_tasks", JsonConvert.SerializeObject(tasks));

            return tasks;
        }
    }
}
