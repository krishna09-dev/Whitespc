namespace whitespc.Models;

public enum MoodCategory
{
    Positive,
    Neutral,
    Negative
}

public enum MoodType
{
    // Positive moods
    Happy,
    Excited,
    Relaxed,
    Grateful,
    Confident,

    // Neutral moods
    Calm,
    Thoughtful,
    Curious,
    Nostalgic,
    Bored,

    // Negative moods
    Sad,
    Angry,
    Stressed,
    Lonely,
    Anxious
}

public static class MoodExtensions
{
    public static MoodCategory GetCategory(this MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy or MoodType.Excited or MoodType.Relaxed or MoodType.Grateful or MoodType.Confident => MoodCategory.Positive,
            MoodType.Calm or MoodType.Thoughtful or MoodType.Curious or MoodType.Nostalgic or MoodType.Bored => MoodCategory.Neutral,
            _ => MoodCategory.Negative
        };
    }

    public static string GetEmoji(this MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy => "😊",
            MoodType.Excited => "🤩",
            MoodType.Relaxed => "😌",
            MoodType.Grateful => "🙏",
            MoodType.Confident => "😎",
            MoodType.Calm => "🧘",
            MoodType.Thoughtful => "🤔",
            MoodType.Curious => "🧐",
            MoodType.Nostalgic => "🥲",
            MoodType.Bored => "😐",
            MoodType.Sad => "😢",
            MoodType.Angry => "😡",
            MoodType.Stressed => "😫",
            MoodType.Lonely => "🥺",
            MoodType.Anxious => "😰",
            _ => "❓"
        };
    }

    public static string GetColor(this MoodType mood)
    {
        return mood.GetCategory() switch
        {
            MoodCategory.Positive => "emerald",
            MoodCategory.Neutral => "amber",
            MoodCategory.Negative => "rose",
            _ => "slate"
        };
    }
}
