using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, List<TaskEntity>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public GetAllTasksQueryHandler(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        public async Task<List<TaskEntity>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            return await _context.Tasks.ToListAsync();
        }
    }
}
