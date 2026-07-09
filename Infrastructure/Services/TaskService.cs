using System.Net;
using Domain.DTOs;
using Domain.TaskEntity;
using Domain.TaskEnum;
using Domain.TaskStatusFilter;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class TaskService(ApplicationDbContext dbContext) : ITaskService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> AddAsync(TaskDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return new Response<string>(HttpStatusCode.BadRequest, "Title is required");
        }

        var maxOrderIndex = await context.Tasks.MaxAsync(t => (int?)t.OrderIndex) ?? 0;
        var utcNow = DateTime.UtcNow;

        var task = new TaskEntity
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            OrderIndex = maxOrderIndex + 1,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await context.Tasks.AddAsync(task);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add Task successfully");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.Tasks.FindAsync(id);
        if (del == null)
        {
            return new Response<string>(HttpStatusCode.NotFound, "Task not found");
        }

        context.Tasks.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted Task successfully");
    }

    public async Task<PagedResult<TaskEntity>> GetAllAsync(TaskStatusFilter filter, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = context.Tasks.AsQueryable();

        if (filter != null && filter.Status != null)
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.OrderIndex)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TaskEntity>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<Response<TaskEntity>> GetByIdAsync(int id)
    {
        var get = await context.Tasks.FindAsync(id);
        if (get == null)
        {
            return new Response<TaskEntity>(HttpStatusCode.NotFound, "Task not found");
        }

        return new Response<TaskEntity>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<string>> UpdateAsync(int id, TaskDTO dto)
    {
        var update = await context.Tasks.FindAsync(id);
        if (update == null)
        {
            return new Response<string>(HttpStatusCode.NotFound, "Task not found");
        }

        update.Title = dto.Title;
        update.Description = dto.Description;
        update.Status = dto.Status;
        update.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> UpdateStatusAsync(int id, TaskEnum status)
    {
        var update = await context.Tasks.FindAsync(id);
        if (update == null)
        {
            return new Response<string>(HttpStatusCode.NotFound, "Task not found");
        }

        update.Status = status;
        update.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Status updated successfully");
    }

    public async Task<Response<string>> ReorderAsync(List<TaskDTO> items)
    {
        if (items == null || items.Count == 0)
        {
            return new Response<string>(HttpStatusCode.BadRequest, "Reorder list is empty");
        }

        var taskIds = items.Select(i => i.Id).ToList();
        var tasks = await context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        if (tasks.Count != items.Count)
        {
            return new Response<string>(HttpStatusCode.NotFound, "One or more tasks not found");
        }

        var orderById = items.ToDictionary(i => i.Id, i => i.OrderIndex);
        var utcNow = DateTime.UtcNow;

        foreach (var task in tasks)
        {
            task.OrderIndex = orderById[task.Id];
            task.UpdatedAt = utcNow;
        }

        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Tasks reordered successfully");
    }
}
