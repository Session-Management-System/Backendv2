namespace Session_Management_System.DTOs
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
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

    public class CompletedSessionDto
    {
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public int RemainingCapacity { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string? SessionLink { get; set; }
    }

    public class UserSessionStatsDto
    {
        public int TotalBookings { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
    }
}
