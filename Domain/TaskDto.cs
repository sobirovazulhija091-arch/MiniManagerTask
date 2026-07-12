namespace Domain.DTOs;

public class TaskDTO
{
    public string? Title { get; set; }=null;
    public string? Description { get; set; }=null;
    public TaskEnum.TaskEnum Status { get; set; }=TaskEnum.TaskEnum.ToDo;
}
