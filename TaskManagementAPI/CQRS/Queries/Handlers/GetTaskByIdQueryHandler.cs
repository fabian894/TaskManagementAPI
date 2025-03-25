using MediatR;
using StackExchange.Redis;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskEntity>
    {
        private readonly ApplicationDbContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<GetTaskByIdQueryHandler> _logger;

        public GetTaskByIdQueryHandler(ApplicationDbContext context, IConnectionMultiplexer redis, ILogger<GetTaskByIdQueryHandler> logger)
        {
            _context = context;
            _redis = redis;
            _logger = logger;
        }

        public async Task<TaskEntity> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching task with ID {TaskId} from cache or database.", request.Id);

            var cacheKey = $"task_{request.Id}";
            var cache = _redis.GetDatabase();

            var cachedTask = await cache.StringGetAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedTask))
            {
                _logger.LogInformation("Task with ID {TaskId} found in cache.", request.Id);
                return JsonConvert.DeserializeObject<TaskEntity>(cachedTask);
            }

            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found in the database.", request.Id);
                return null;
            }

            // Cache the task for future requests
            var taskJson = JsonConvert.SerializeObject(task);
            await cache.StringSetAsync(cacheKey, taskJson, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Task with ID {TaskId} fetched from database and cached.", request.Id);

            return task;
        }
    }
}
