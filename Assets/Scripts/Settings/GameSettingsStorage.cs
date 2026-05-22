using System;
using System.IO;
using UnityEngine;

namespace Core.Settings
{
    public sealed class GameSettingsStorage
    {
        private const string FileName = "settings.json";

        public string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public GameSettingsData LoadOrDefault() => Read() ?? new GameSettingsData();

        public GameSettingsData Read()
        {
            if (!File.Exists(SavePath))
                return null;

            try
            {
                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<GameSettingsData>(json);
                data?.Normalize();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSettingsStorage] Failed to read settings: {e.Message}. Using defaults.");
                return null;
            }
        }

        public void Write(GameSettingsData data)
        {
            if (data == null)
                return;

            try
            {
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var copy = data.Clone();
                copy.Normalize();

                var json = JsonUtility.ToJson(copy, prettyPrint: true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSettingsStorage] Failed to write settings: {e.Message}");
            }
        }
    }
}
