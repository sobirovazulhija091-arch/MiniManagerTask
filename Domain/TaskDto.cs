namespace Domain.DTOs;

public class TaskDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }=null;
    public string? Description { get; set; }=null;
    public TaskEnum.TaskEnum Status { get; set; }=TaskEnum.TaskEnum.ToDo;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;

}
