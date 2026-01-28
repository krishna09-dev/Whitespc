using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace whitespc.Models;

public class JournalEntry
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateOnly EntryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Required]
    public MoodType PrimaryMood { get; set; }

    public MoodType? SecondaryMood1 { get; set; }

    public MoodType? SecondaryMood2 { get; set; }

    public int WordCount { get; set; }

    // Favorites / Pinned / Archived Entries
    public bool IsPinned { get; set; } = false;
    public bool IsFavorite { get; set; } = false;
    public bool IsArchived { get; set; } = false;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    [NotMapped]
    public MoodCategory PrimaryMoodCategory => PrimaryMood.GetCategory();

    [NotMapped]
    public string PrimaryMoodEmoji => PrimaryMood.GetEmoji();

    [NotMapped]
    public List<MoodType> AllMoods
    {
        get
        {
            var moods = new List<MoodType> { PrimaryMood };
            if (SecondaryMood1.HasValue) moods.Add(SecondaryMood1.Value);
            if (SecondaryMood2.HasValue) moods.Add(SecondaryMood2.Value);
            return moods;
        }
    }

    public void UpdateWordCount()
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            WordCount = 0;
            return;
        }

        // Strip HTML tags from content before counting words
        var textContent = System.Text.RegularExpressions.Regex.Replace(Content, "<[^>]+>", " ");
        textContent = System.Net.WebUtility.HtmlDecode(textContent);
        
        WordCount = textContent.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
