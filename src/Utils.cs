namespace DiscordGmail.Utils;

using System;
using System.Text;
using System.Text.RegularExpressions;

// Does not work lol
public static class DiscordTokenValidator
{
    // Base64 URL-safe pattern (no padding '=')
    private static readonly Regex Base64UrlRegex = new Regex(@"^[A-Za-z0-9\-_]+$", RegexOptions.Compiled);

    public static bool IsValidFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        // Split into 3 parts
        string[] parts = token.Split('.');
        if (parts.Length != 3)
            return false;

        string part1 = parts[0];
        string part2 = parts[1];
        string part3 = parts[2];

        // Validate base64-url format for parts
        if (!Base64UrlRegex.IsMatch(part1) ||
            !Base64UrlRegex.IsMatch(part2) ||
            !Base64UrlRegex.IsMatch(part3))
        {
            return false;
        }

        // Check if first part decodes to a valid ulong (Discord Snowflake)
        if (!TryBase64UrlDecodeToUInt64(part1, out ulong _))
            return false;

        // Optionally, check if second part decodes at all (not necessarily ulong)
        if (!TryBase64UrlDecode(part2, out _))
            return false;

        // Third part can be any base64url string (just validate format above)
        return true;
    }

    private static bool TryBase64UrlDecode(string input, out byte[] output)
    {
        output = null;
        try
        {
            string padded = PadBase64Url(input);
            output = Convert.FromBase64String(padded);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryBase64UrlDecodeToUInt64(string input, out ulong result)
    {
        result = 0;
        if (!TryBase64UrlDecode(input, out byte[] bytes))
            return false;

        // Discord Snowflake is a 64-bit unsigned integer (big-endian)
        if (bytes.Length > 8) // Shouldn't be longer than 8 bytes
            return false;

        // Pad to 8 bytes if needed
        byte[] buffer = new byte[8];
        int offset = buffer.Length - bytes.Length;
        Array.Copy(bytes, 0, buffer, offset, bytes.Length);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(buffer);

        result = BitConverter.ToUInt64(buffer, 0);
        return true;
    }

    private static string PadBase64Url(string base64Url)
    {
        // Base64URL has no padding, Base64 requires padding to multiple of 4
        int paddingNeeded = 4 - (base64Url.Length % 4);
        if (paddingNeeded == 4)
            return base64Url;

        return base64Url + new string('=', paddingNeeded);
    }
}
