using Microsoft.EntityFrameworkCore;
using whitespc.Data;
using whitespc.Models;

namespace whitespc.Services;

public class TagService
{
    private readonly JournalDbContext _context;

    public TagService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<List<Tag>> GetPrebuiltTagsAsync()
    {
        return await _context.Tags
            .Where(t => t.IsPrebuilt)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetCustomTagsAsync()
    {
        return await _context.Tags
            .Where(t => !t.IsPrebuilt)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<Tag> CreateTagAsync(string name)
    {
        var existingTag = await GetTagByNameAsync(name);
        if (existingTag != null)
            throw new InvalidOperationException("A tag with this name already exists.");

        var tag = new Tag
        {
            Name = name.Trim(),
            IsPrebuilt = false
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return tag;
    }

    public async Task DeleteTagAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag != null && !tag.IsPrebuilt)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Tag>> GetTagsByIdsAsync(List<int> ids)
    {
        return await _context.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }
}
