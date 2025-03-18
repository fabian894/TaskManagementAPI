using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Handlers
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskEntity>
    {
        private readonly ApplicationDbContext _context;

        public UpdateTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskEntity> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null) return null;

            task.Title = request.Title;
            task.Description = request.Description;

            if (Enum.TryParse<TaskStatus>(request.Status, out var status))
            {
                task.Status = status;
            }
            else
            {
                throw new ArgumentException($"Invalid status: {request.Status}");
            }

            task.DueDate = request.DueDate;

            await _context.SaveChangesAsync();
            return task;
        }
    }
}
