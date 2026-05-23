using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.StateMachine.Features;
using Core.TestSkillTree;
using UnityEngine;

namespace Core.Save
{
    public class SaveService
    {
        private SaveData _data;
        private readonly List<ISaveable> _saveables;
        private readonly SaveStorage _storage = new SaveStorage();

        public SaveData GetData() => _data;

        public SaveService(IEnumerable<ISaveable> saveables, GameplayFeature gameplayFeature,
            SkillTreeService skillTreeService, DungeonSelectionService dungeonSelectionService)
        {
            _saveables = new List<ISaveable>(saveables);
            _data = _storage.LoadOrDefault();

            foreach (var saveable in _saveables)
                saveable.Load(_data);

            gameplayFeature.OnSessionExpired += Save;
            gameplayFeature.OnDemoLimitReached += (_, _, _) => Save();
            skillTreeService.OnUpgraded += Save;
            dungeonSelectionService.OnProgressChanged += Save;

            Debug.Log($"[SaveService] Loaded. Version: {_data.Version}, Path: {_storage.SavePath}");
        }

        public void LoadFor(ISaveable saveable) => saveable.Load(_data);

        public void Save()
        {
            _data = new SaveData();
            
            foreach (var saveable in _saveables)
                saveable.Contribute(_data);
            
            _storage.Write(_data);
        }

        public void Reset()
        {
            _storage.Delete();

            _data = new SaveData();
            foreach (var saveable in _saveables)
                saveable.Load(_data);

            Debug.Log("[SaveService] Save reset.");
        }
    }
}
