using MediatR;
using StackExchange.Redis;
using System.Text.Json;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskEntity>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;

        public GetTaskByIdQueryHandler(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        public async Task<TaskEntity> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"task_{request.Id}";

            // Check cache first
            var cachedTask = await _cache.StringGetAsync(cacheKey);
            if (!cachedTask.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<TaskEntity>(cachedTask);
            }

            // If not in cache, fetch from DB
            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null) return null;

            // Store in cache for 10 minutes
            await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(task), TimeSpan.FromMinutes(10));

            return task;
        }
    }
}
