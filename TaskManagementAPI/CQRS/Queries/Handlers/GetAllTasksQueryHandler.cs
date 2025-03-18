using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, List<TaskEntity>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;

        public GetAllTasksQueryHandler(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        public async Task<List<TaskEntity>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            const string cacheKey = "all_tasks";

            // Check Redis cache first
            var cachedTasks = await _cache.StringGetAsync(cacheKey);
            if (!cachedTasks.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<TaskEntity>>(cachedTasks);
            }

            // If not in cache, fetch from DB
            var tasks = await _context.Tasks.ToListAsync();

            // Store in Redis for future requests (set expiry to 10 mins)
            await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(tasks), TimeSpan.FromMinutes(10));

            return tasks;
        }
    }
}
