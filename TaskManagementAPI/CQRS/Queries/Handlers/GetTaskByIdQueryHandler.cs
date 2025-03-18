using MediatR;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Queries.Handlers
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskEntity>
    {
        private readonly ApplicationDbContext _context;

        public GetTaskByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskEntity> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Tasks.FindAsync(request.Id);
        }
    }
}
