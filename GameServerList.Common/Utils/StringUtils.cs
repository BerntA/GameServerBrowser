﻿using System.Text;

namespace GameServerList.Common.Utils;

public static class StringUtils
{
    public static string ReadNullTerminatedString(ref BinaryReader input)
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
}