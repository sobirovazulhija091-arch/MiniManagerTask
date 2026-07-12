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

        int? maxOrderIndexNullable = await context.Tasks.MaxAsync(t => (int?)t.OrderIndex);
        int maxOrderIndex = maxOrderIndexNullable ?? 0;

        var task = new TaskEntity
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            OrderIndex = maxOrderIndex + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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

    public async Task<PagedResult<TaskEntity>> GetAllAsync(TaskStatusFilter filter, PagedQuery pagedQuery)
    {
        if (pagedQuery.Page < 1) pagedQuery.Page = 1;
        if (pagedQuery.PageSize < 1) pagedQuery.PageSize = 10;

        var query = context.Tasks.AsNoTracking().AsQueryable();

        if (filter != null && filter.Status != null)
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.OrderIndex)
            .ThenBy(t => t.Id)
            .Skip((pagedQuery.Page - 1) * pagedQuery.PageSize)
            .Take(pagedQuery.PageSize)
            .ToListAsync();

        return new PagedResult<TaskEntity>
        {
            Items = items,
            Page = pagedQuery.Page,
            PageSize = pagedQuery.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagedQuery.PageSize)
        };
    }

    public async Task<Response<TaskEntity>> GetByIdAsync(int id)
    {
        var get = await context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (get == null)
        {
            return new Response<TaskEntity>(HttpStatusCode.NotFound, "Task not found");
        }

        return new Response<TaskEntity>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<string>> UpdateAsync(int id, TaskDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return new Response<string>(HttpStatusCode.BadRequest, "Title is required");
        }

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
        return new Response<string>(HttpStatusCode.OK, "Update Task successfully");
    }

    public async Task<Response<string>> UpdateStatusAsync(int id, TaskEnum status)
    {
        if (!Enum.IsDefined(typeof(TaskEnum), status))
        {
            return new Response<string>(HttpStatusCode.BadRequest, "Invalid status");
        }

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

    public async Task<Response<string>> ReorderAsync(List<TaskReorderDto> items)
    {
        if (items == null || items.Count == 0)
        {
            return new Response<string>(HttpStatusCode.BadRequest, "List reorder is empty");
        }

        var taskIds = items.Select(i => i.Id).ToList();
        var tasks = await context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        if (tasks.Count != items.Count)
        {
            return new Response<string>(HttpStatusCode.NotFound, "Some tasks not found");
        }

        var utcNow = DateTime.UtcNow;

        foreach (var task in tasks)
        {
            var matchingItem = items.FirstOrDefault(i => i.Id == task.Id);

            if (matchingItem != null)
            {
                task.OrderIndex = matchingItem.OrderIndex;
                task.UpdatedAt = utcNow;
            }
        }

        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Tasks reordered successfully");
    }
}
