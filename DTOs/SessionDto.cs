// SessionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Session_Management_System.DTOs
{

    public class SessionResponseDto
    {
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public int RemainingCapacity { get; set; }
        public bool IsApproved { get; set; }
        public string? SessionLink { get; set; }
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
    }

    public class SessionUpdateDto
    {

        [Required]
        public int SessionId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string? Description { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }
        [Required]
        public DateTime? EndTime { get; set; }
        [Required]
        public int? Capacity { get; set; }
        [Required]
        public string? SessionLink { get; set; }

    }
}
