﻿using Newtonsoft.Json;

namespace GameServerList.Common.Utils;

public static class FileUtils
{
    public static T LoadDataFromFile<T>(string path)
    {
        if (!File.Exists(path))
            return default;

        var content = File.ReadAllText(path);
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonConvert.DeserializeObject<T>(content);
    }

    public static void WriteDataToFile<T>(string path, T data)
    {
        var content = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, content);
    }
}