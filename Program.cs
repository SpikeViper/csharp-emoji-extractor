using System.Globalization;
using System.Text;
using System.Text.Json;

public class Program
{
    private static readonly HashSet<int> EmojiCodePoints = new HashSet<int>();
    
    public static void Main(string[] args)
    {
        Init(); // Initialize the emoji code points
        
        while (true)
        {
            Console.WriteLine("Enter emoji:");
            string emoji = Console.ReadLine();
            Console.WriteLine(JsonSerializer.Serialize(GetEmojiList(emoji)));
        }
    }

    public static void Init()
    {
        AddCachedRange(0x1F600, 0x1F64F); // Emoticons
        AddCachedRange(0x2600, 0x26FF); // Misc Symbols
        AddCachedRange(0x2700, 0x27BF); // Dingbats
        AddCachedRange(0x1F300, 0x1F5FF); // Other emojis
        AddCachedRange(0x1F900, 0x1F9FF); // Supplemental Symbols and Pictographs
        AddCachedRange(0x1F680, 0x1F6FF); // Transport and Map Symbols
        AddCachedRange(0x1F700, 0x1F77F); // Alchemical Symbols
        AddCachedRange(0x1FA70, 0x1FAFF); // Symbols and Pictographs Extended-A
    }

    public static void AddCachedRange(int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            EmojiCodePoints.Add(i);
        }
    }
    
    public static bool IsEmoji(string input, int index)
    {
        // Early exit for ASCII characters
        if (input[index] <= 127)
        {
            return false;
        }

        int codePoint;
        if (Char.IsHighSurrogate(input[index]) && index < input.Length - 1 && Char.IsLowSurrogate(input[index + 1]))
        {
            codePoint = Char.ConvertToUtf32(input, index);
        }
        else
        {
            codePoint = input[index];
        }

        return EmojiCodePoints.Contains(codePoint);
    }

    public static bool IsJoiner(char c)
    {
        int codePoint = Convert.ToInt32(c);
        return codePoint == 0x200D || codePoint == 0xFE0F;
    }

    public static bool IsModifier(char c)
    {
        int codePoint = Convert.ToInt32(c);
        return codePoint >= 0x1F3FB && codePoint <= 0x1F3FF;
    }

    public static List<string> GetEmojiList(string s)
    {
        var emojis = new List<string>();
        for (int i = 0; i < s.Length; i++)
        {
            var emoji = GetEmoji(s, i);
            if (emoji != null)
            {
                emojis.Add(emoji);
                // Skip the length of the emoji minus one, since the for-loop will increment 'i'
                i += Math.Max(0, emoji.Length - 1);
                if (Char.IsHighSurrogate(s[i]))
                {
                    i++; // Skip the low surrogate part of the pair
                }
            }
        }
        return emojis;
    }

    public static string GetEmoji(string input, int index)
    {
        // Check if the index is valid and the character at the index is an emoji
        if (input.Length <= index || !IsEmoji(input, index))
        {
            return null;
        }

        StringBuilder emoji = new StringBuilder();
        bool lastWasEmoji = true;

        for (int i = index; i < input.Length; i++)
        {
            char currentChar = input[i];
            bool isHighSurrogate = Char.IsHighSurrogate(currentChar);
            bool isLowSurrogate = i > index && Char.IsLowSurrogate(currentChar);

            // Check for surrogate pairs
            if (isHighSurrogate && i < input.Length - 1 && Char.IsLowSurrogate(input[i + 1]))
            {
                // Append the entire surrogate pair
                emoji.Append(currentChar).Append(input[i + 1]);
                i++; // Skip the next character (low surrogate)
                lastWasEmoji = true;
            }
            else if (!isLowSurrogate) // Process non-surrogate or low surrogate (if it's not part of a pair)
            {
                if (lastWasEmoji && (IsJoiner(currentChar) || IsModifier(currentChar)))
                {
                    emoji.Append(currentChar);
                    lastWasEmoji = IsJoiner(currentChar);
                }
                else if (!lastWasEmoji && IsEmoji(input, i))
                {
                    emoji.Append(currentChar);
                    lastWasEmoji = true;
                }
                else
                {
                    break;
                }
            }
        }

        return emoji.ToString();
    }
}