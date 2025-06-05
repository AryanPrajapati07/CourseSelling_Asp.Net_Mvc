namespace Demo.Models
{
    public class VideoProgress
    {
        public int Id { get; set; }
        public string StudentEmail { get; set; }
        public int CourseId { get; set; }
        public double WatchedSeconds { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
