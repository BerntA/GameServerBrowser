using System.Text;

namespace GameServerList.Common.Utils;

public static class StringUtils
{
    public static string Truncate(this string value, int maxLength, string truncationSuffix = "…")
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length > maxLength)
            return $"{value[..maxLength]}{truncationSuffix}";

        return value;
    }

    public static string ReadNullTerminatedString(BinaryReader input)
    {
        var sb = new StringBuilder();
        var read = input.ReadChar();
        while (read != '\x00')
        {
            sb.Append(read);
            read = input.ReadChar();
        }
        return sb.ToString();
    }

    public static byte[] WriteNullTerminatedString(string value)
    {
        return string.IsNullOrEmpty(value) ? [0x00] : [.. Encoding.UTF8.GetBytes(value), 0x00];
    }
}