using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
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

        public TaskController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
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
        public async Task<IActionResult> CreateTask([FromBody] TaskEntity task)
        {
            if (task == null)
                return BadRequest("Invalid task data.");

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
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
            return await _context.Tasks.ToListAsync();
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
            string cacheKey = $"task_{id}";
            string cachedTask = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedTask))
            {
                var task = JsonConvert.DeserializeObject<TaskEntity>(cachedTask);
                return Ok(task);
            }

            var taskFromDb = await _context.Tasks.FindAsync(id);

            if (taskFromDb == null)
            {
                return NotFound("Task not found.");
            }

            var taskJson = JsonConvert.SerializeObject(taskFromDb);
            await _cache.SetStringAsync(cacheKey, taskJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return Ok(taskFromDb);
        }

        /// <summary>
        /// Updates the status of a task.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <param name="trigger">The transition trigger.</param>
        /// <returns>The updated task status.</returns>
        [HttpPut("{id}/status")]
        [SwaggerOperation(Summary = "Update task status", Description = "Changes the status of an existing task.")]
        [SwaggerResponse(200, "Task status updated successfully")]
        [SwaggerResponse(400, "Invalid status transition")]
        [SwaggerResponse(404, "Task not found")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] int newStatus)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound("Task not found.");

            if (!Enum.IsDefined(typeof(TaskStatus), newStatus))
                return BadRequest("Invalid status value.");

            var newTaskStatus = (TaskStatus)newStatus;

            // Determine the trigger based on status change
            TaskTrigger? trigger = null;

            if (task.Status == TaskStatus.Pending && newTaskStatus == TaskStatus.InProgress)
                trigger = TaskTrigger.Start;
            else if (task.Status == TaskStatus.InProgress && newTaskStatus == TaskStatus.Completed)
                trigger = TaskTrigger.Complete;
            else
                return BadRequest($"Invalid status transition from {task.Status} to {newTaskStatus}.");

            task.MoveTo(trigger.Value);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Task moved to {task.Status}" });
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
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound("Task not found.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
