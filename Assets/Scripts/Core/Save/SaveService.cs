using System;
using System.Collections.Generic;
using System.IO;
using Core.StateMachine.Features;
using Core.TestSkillTree;
using UnityEngine;

namespace Core.Save
{
    public class SaveService
    {
        private SaveData _data;
        private readonly List<ISaveable> _saveables;

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public SaveData GetData() => _data;

        public SaveService(IEnumerable<ISaveable> saveables, GameplayFeature gameplayFeature, SkillTreeService skillTreeService)
        {
            _saveables = new List<ISaveable>(saveables);
            _data = ReadFromDisk() ?? new SaveData();

            foreach (var saveable in _saveables)
                saveable.Load(_data);

            gameplayFeature.OnSessionExpired += Save;
            skillTreeService.OnUpgraded += Save;

            Debug.Log($"[SaveService] Loaded. Version: {_data.Version}, Path: {SavePath}");
        }

        public void LoadFor(ISaveable saveable) => saveable.Load(_data);

        public void Save()
        {
            _data = new SaveData();
            
            foreach (var saveable in _saveables)
                saveable.Contribute(_data);
            
            WriteToDisk(_data);
        }

        public void Reset()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            _data = new SaveData();
            foreach (var saveable in _saveables)
                saveable.Load(_data);

            Debug.Log("[SaveService] Save reset.");
        }

        private SaveData ReadFromDisk()
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
                Debug.LogWarning($"[SaveService] Failed to read save file: {e.Message}. Starting fresh.");
                return null;
            }
        }

        private void WriteToDisk(SaveData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to write save file: {e.Message}");
            }
        }
    }
}
