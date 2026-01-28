namespace whitespc.Services;

public class MotivationService
{
    private static readonly List<string> Quotes = new()
    {
        "The only way to do great work is to love what you do. — Steve Jobs",
        "Believe you can and you're halfway there. — Theodore Roosevelt",
        "Your limitation—it's only your imagination.",
        "Push yourself, because no one else is going to do it for you.",
        "Great things never come from comfort zones.",
        "Dream it. Wish it. Do it.",
        "Success doesn't just find you. You have to go out and get it.",
        "The harder you work for something, the greater you'll feel when you achieve it.",
        "Don't stop when you're tired. Stop when you're done.",
        "Wake up with determination. Go to bed with satisfaction.",
        "Do something today that your future self will thank you for.",
        "Little things make big days.",
        "It's going to be hard, but hard does not mean impossible.",
        "Don't wait for opportunity. Create it.",
        "Sometimes we're tested not to show our weaknesses, but to discover our strengths.",
        "The key to success is to focus on goals, not obstacles.",
        "Dream bigger. Do bigger.",
        "Don't be afraid to give up the good to go for the great. — John D. Rockefeller",
        "Write it on your heart that every day is the best day in the year. — Ralph Waldo Emerson",
        "Start where you are. Use what you have. Do what you can. — Arthur Ashe",
        "The secret of getting ahead is getting started. — Mark Twain",
        "Your journal is a safe space. Pour your heart into it.",
        "Today is a new page. Make it a good one.",
        "Reflection is the lamp of the heart. — Al-Hasan",
        "In the journal I do not just express myself more openly than I could to any person; I create myself.",
        "Writing in a journal reminds you of your goals and of your learning in life.",
        "Journal writing is a voyage to the interior. — Christina Baldwin",
        "Fill your paper with the breathings of your heart. — William Wordsworth",
        "The act of writing is the act of discovering what you believe. — David Hare",
        "Keep a diary, and someday it'll keep you. — Mae West"
    };

    public string GetDailyQuote()
    {
        // Use the day of year as seed for consistent daily quote
        var dayOfYear = DateTime.Now.DayOfYear;
        var index = dayOfYear % Quotes.Count;
        return Quotes[index];
    }

    public string GetRandomQuote()
    {
        var random = new Random();
        return Quotes[random.Next(Quotes.Count)];
    }
}
