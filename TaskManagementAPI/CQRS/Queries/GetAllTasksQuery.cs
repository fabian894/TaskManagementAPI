using MediatR;
using TaskManagementAPI.Models;
using System.Collections.Generic;

namespace TaskManagementAPI.CQRS.Queries
{
    public class GetAllTasksQuery : IRequest<List<TaskEntity>> { }
}
