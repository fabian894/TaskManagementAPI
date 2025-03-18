using MediatR;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Commands
{
    public class CreateTaskCommand : IRequest<TaskEntity>
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

        public CreateTaskCommand(string title, string? description, string status, DateTime dueDate)
        {
            Title = title;
            Description = description;
            Status = status;
            DueDate = dueDate;
        }
    }
}
