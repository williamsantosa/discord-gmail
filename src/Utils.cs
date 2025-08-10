namespace DiscordGmail.Utils;

using System;
using System.Text.RegularExpressions;

public static class DiscordTokenValidator
{
    // Approximate regex for a Discord bot token format
    private static readonly Regex TokenPattern = new Regex(
        @"^[\w-]{24}\.[\w-]{6}\.[\w-]{27}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Checks if the token matches the typical Discord token structure.
    /// </summary>
    public static bool IsValidFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return TokenPattern.IsMatch(token);
    }
}
