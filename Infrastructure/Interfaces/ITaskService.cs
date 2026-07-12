using Domain.DTOs;
using Domain.TaskEntity;
using Domain.TaskEnum;
using Domain.TaskStatusFilter;

public interface ITaskService
{
    Task<Response<string>> AddAsync(TaskDTO dto);
    Task<Response<string>> UpdateAsync(int id, TaskDTO dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<PagedResult<TaskEntity>> GetAllAsync(TaskStatusFilter filter, PagedQuery pagedQuery);
    Task<Response<TaskEntity>> GetByIdAsync(int id);
    Task<Response<string>> UpdateStatusAsync(int id, TaskEnum status);
    Task<Response<string>> ReorderAsync(List<TaskReorderDto> items);
}
