using MediatR;
using StackExchange.Redis;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskEntity>
    {
        private readonly ApplicationDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public GetTaskByIdQueryHandler(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        public async Task<TaskEntity> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Tasks.FindAsync(request.Id);
        }
    }
}
