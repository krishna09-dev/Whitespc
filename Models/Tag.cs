using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace whitespc.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public bool IsPrebuilt { get; set; }

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

    public static List<string> PrebuiltTags => new()
    {
        "Work", "Career", "Studies", "Family", "Friends", "Relationships",
        "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies",
        "Travel", "Nature", "Finance", "Spirituality", "Birthday",
        "Holiday", "Vacation", "Celebration", "Exercise", "Reading",
        "Writing", "Cooking", "Meditation", "Yoga", "Music",
        "Shopping", "Parenting", "Projects", "Planning", "Reflection"
    };
}
