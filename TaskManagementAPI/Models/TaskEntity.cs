using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using Stateless;
using System.Text.Json.Serialization;

namespace TaskManagementAPI.Models
{

    public class TaskEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatus Status { get; set; }  

        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

        private readonly StateMachine<TaskStatus, TaskTrigger> _stateMachine;

        public TaskEntity()
        {
            _stateMachine = new StateMachine<TaskStatus, TaskTrigger>(() => Status, s => Status = s);

            _stateMachine.Configure(TaskStatus.Pending)
                .Permit(TaskTrigger.Start, TaskStatus.InProgress);

            _stateMachine.Configure(TaskStatus.InProgress)
                .Permit(TaskTrigger.Complete, TaskStatus.Completed);
        }

        public bool CanMoveTo(TaskTrigger trigger) => _stateMachine.CanFire(trigger);

        public void MoveTo(TaskTrigger trigger)
        {
            if (!_stateMachine.CanFire(trigger))
                throw new InvalidOperationException($"Cannot transition from {Status} using {trigger}");

            _stateMachine.Fire(trigger);
        }
    }

    public enum TaskTrigger
    {
        Start,      
        Complete   
    }
}
