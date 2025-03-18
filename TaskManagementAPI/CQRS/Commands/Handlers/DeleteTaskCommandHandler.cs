using MediatR;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using StackExchange.Redis;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;

        public DeleteTaskCommandHandler(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            await _cache.KeyDeleteAsync($"task_{request.Id}");
            await _cache.KeyDeleteAsync("all_tasks");

            return true;
        }
    }
}
