using System;
using System.IO;
using UnityEngine;

namespace Core.Save
{
    public class SaveStorage
    {
        private const string FileName = "save.json";

        public string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public bool HasSave() => File.Exists(SavePath);

        public SaveData LoadOrDefault() => Read() ?? new SaveData();

        public SaveData Read()
        {
            if (!File.Exists(SavePath))
                return null;

            try
            {
                var json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveStorage] Failed to read save file: {e.Message}. Starting fresh.");
                return null;
            }
        }

        public void Write(SaveData data)
        {
            try
            {
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveStorage] Failed to write save file: {e.Message}");
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(SavePath))
                    File.Delete(SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveStorage] Failed to delete save file: {e.Message}");
            }
        }
    }
}
