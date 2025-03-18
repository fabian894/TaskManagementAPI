using MediatR;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.CQRS.Queries
{
    public class GetTaskByIdQuery : IRequest<TaskEntity>
    {
        public int Id { get; set; }
        public GetTaskByIdQuery(int id) => Id = id;
    }
}
