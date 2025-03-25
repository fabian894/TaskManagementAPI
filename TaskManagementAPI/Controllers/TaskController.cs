using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagementAPI.CQRS.Commands;
using TaskManagementAPI.CQRS.Queries;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IMediator _mediator;

        public TaskController(ApplicationDbContext context, IDistributedCache cache, IMediator mediator)
        {
            _context = context;
            _cache = cache;
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="task">The task entity to create.</param>
        /// <returns>The created task.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Create a new task", Description = "Adds a new task to the system.")]
        [SwaggerResponse(201, "Task created successfully", typeof(TaskEntity))]
        [SwaggerResponse(400, "Invalid task data")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command, [FromServices] IMediator mediator)
        {
            if (command == null)
                return BadRequest("Invalid task data.");

            var task = await mediator.Send(command);

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }
        /// <summary>
        /// Retrieves all tasks.
        /// </summary>
        /// <returns>A list of all tasks.</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Get all tasks", Description = "Returns a list of all tasks.")]
        [SwaggerResponse(200, "Returns the list of tasks", typeof(IEnumerable<TaskEntity>))]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetAllTasks()
        {
            var query = new GetAllTasksQuery();
            var tasks = await _mediator.Send(query);

            if (tasks == null || !tasks.Any())
            {
                return NotFound("No tasks found.");
            }

            return Ok(tasks);
        }


        /// <summary>
        /// Retrieves a task by ID.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <returns>The task if found, otherwise 404.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a task by ID", Description = "Retrieves a task from the database using the given ID.")]
        [SwaggerResponse(200, "Task found", typeof(TaskEntity))]
        [SwaggerResponse(404, "Task not found")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var query = new GetTaskByIdQuery(id);
            var task = await _mediator.Send(query);

            if (task == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(task);
        }


        /// <summary>
        /// Updates the status of a task.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <param name="trigger">The transition trigger.</param>
        /// <returns>The updated task status.</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update task details", Description = "Allows updating task title, description, and status while maintaining valid transitions.")]
        [SwaggerResponse(200, "Task updated successfully")]
        [SwaggerResponse(400, "Invalid status transition or data")]
        [SwaggerResponse(404, "Task not found")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Task ID mismatch.");
            }

            var updatedTask = await _mediator.Send(command);

            if (updatedTask == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(new { message = $"Task updated successfully. Current status: {updatedTask.Status}" });
        }
        /// <summary>
        /// Deletes a task by ID.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete a task", Description = "Deletes a task from the system.")]
        [SwaggerResponse(204, "Task deleted successfully")]
        [SwaggerResponse(404, "Task not found")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var command = new DeleteTaskCommand(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound("Task not found.");
            }

            return NoContent(); 
        }
    }
}
