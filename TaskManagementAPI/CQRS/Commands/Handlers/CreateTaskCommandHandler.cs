using MediatR;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskEntity>
    {
        private readonly ApplicationDbContext _context;

        public CreateTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
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
            return task;
        }


    }
}
