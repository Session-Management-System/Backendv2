namespace Session_Management_System.Models{
    public class Session
    {

        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TrainerId { get; set; }
        public int Capacity { get; set; }
        public bool IsApproved { get; set; } = false;
        public string? SessionLink { get; set; }
        public int RemainingCapacity { get; set; }
        public User? Trainer { get; set; }
        
    }
}