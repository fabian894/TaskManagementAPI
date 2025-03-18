using MediatR;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskEntity>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;

        public CreateTaskCommandHandler(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        public async Task<TaskEntity> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
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

            await _cache.KeyDeleteAsync("all_tasks");

            return task;
        }
    }
}
