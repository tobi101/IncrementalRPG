using System;
using System.IO;
using UnityEngine;

namespace Core.Save
{
    public class SaveStorage
    {
        private const string FileName = "save.json";

#if UNITY_EDITOR
        // Editor regression runs use a temporary directory, never the player's real save.
        public static string EditorSaveDirectoryOverride { get; set; }
#endif

        public string SavePath
        {
            get
            {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(EditorSaveDirectoryOverride))
                    return Path.Combine(EditorSaveDirectoryOverride, FileName);
#endif
                return Path.Combine(Application.persistentDataPath, FileName);
            }
        }

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
                var temporaryPath = SavePath + ".tmp";
                File.WriteAllText(temporaryPath, json);
                if (File.Exists(SavePath))
                    File.Replace(temporaryPath, SavePath, null);
                else
                    File.Move(temporaryPath, SavePath);
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
