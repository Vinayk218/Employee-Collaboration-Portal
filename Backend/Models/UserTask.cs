namespace EmployeeCollaborationConsole.Models
{

    public class UserTask
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DueDate { get; set; }
    }
}
