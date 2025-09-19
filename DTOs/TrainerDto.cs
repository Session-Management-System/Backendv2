namespace Session_Management_System.DTOs.TrainerDtos
{
    public class SessionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? SessionLink { get; set; }
    }

    public class SessionResponseDto
    {
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int RemainingCapacity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsApproved { get; set; }
        public string? SessionLink { get; set; }
        public int TrainerId { get; set; }
    }

    public class SessionStatsDto
    {
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
    }
}
