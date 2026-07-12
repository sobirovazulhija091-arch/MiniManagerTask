using Domain.DTOs;
using Domain.TaskEntity;
using Domain.TaskStatusFilter;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskService service) : ControllerBase
{
    [HttpPost]
    public async Task<Response<string>> AddAsync(TaskDTO dto)
    {
        return await service.AddAsync(dto);
    }

    [HttpPut("{id}")]
    public async Task<Response<string>> UpdateAsync(int id, TaskDTO dto)
    {
        return await service.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await service.DeleteAsync(id);
    }

    [HttpGet("{id}")]
    public async Task<Response<TaskEntity>> GetByIdAsync(int id)
    {
        return await service.GetByIdAsync(id);
    }

    [HttpGet]
    public async Task<PagedResult<TaskEntity>> GetAll([FromQuery] TaskStatusFilter filter, [FromQuery] PagedQuery pagedQuery)
    {
        return await service.GetAllAsync(filter, pagedQuery);
    }

    [HttpPatch("{id}/status")]
    public async Task<Response<string>> UpdateStatusAsync(int id, TaskStatusDto dto)
    {
        return await service.UpdateStatusAsync(id, dto.Status);
    }

    [HttpPatch("reorder")]
    public async Task<Response<string>> ReorderAsync(List<TaskReorderDto> items)
    {
        return await service.ReorderAsync(items);
    }
}
