using MediatR;

namespace TaskManagementAPI.CQRS.Commands
{
    public class DeleteTaskCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public DeleteTaskCommand(int id) => Id = id;
    }
}
