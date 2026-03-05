using System.IO;
using UnityEngine;

public static class JsonManager
{
    public static bool TryLoad<T>(string path, out T data) where T : class
    {
        data = null;
        if (!File.Exists(path)) return false;

        try
        {
            var json = File.ReadAllText(path);
            data = JsonUtility.FromJson<T>(json);
            return data != null;
        }
        catch
        {
            data = null;
            return false;
        }
    }

    public static bool TrySave<T>(string path, T data) where T : class
    {
        try
        {
            var json = JsonUtility.ToJson(data, true);

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}