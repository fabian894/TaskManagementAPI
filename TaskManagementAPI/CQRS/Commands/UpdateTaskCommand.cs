using MediatR;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Commands
{
    public class UpdateTaskCommand : IRequest<TaskEntity>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }

        public UpdateTaskCommand(int id, string title, string? description, string status, DateTime dueDate)
        {
            Id = id;
            Title = title;
            Description = description;
            Status = status;
            DueDate = dueDate;
        }
    }
}
