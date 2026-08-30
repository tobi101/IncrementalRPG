using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.Items;
using Core.StateMachine.Features;
using Core.TestSkillTree;
using IncrementalRPG.Scripts.Reflex;
using Model;
using UnityEngine;

namespace Core.Save
{
    public class SaveService : ITickable
    {
        private const float SaveDelay = 0.5f;

        private SaveData _data;
        private readonly List<ISaveable> _saveables;
        private readonly SaveStorage _storage = new SaveStorage();
        private bool _savePending;
        private float _saveDelayRemaining;

        public SaveData GetData() => _data;

        public SaveService(IEnumerable<ISaveable> saveables, GameplayFeature gameplayFeature,
            SkillTreeService skillTreeService, DungeonSelectionService dungeonSelectionService,
            Player player, PlayerItemStorage itemStorage)
        {
            _saveables = new List<ISaveable>(saveables);
            _data = _storage.LoadOrDefault();

            foreach (var saveable in _saveables)
                saveable.Load(_data);

            gameplayFeature.OnSessionExpired += Save;
            gameplayFeature.OnDemoLimitReached += (_, _, _) => Save();
            skillTreeService.OnUpgraded += Save;
            dungeonSelectionService.OnProgressChanged += Save;
            player.OnShardsChanged += ScheduleSave;
            player.OnGoldChanged += ScheduleSave;
            itemStorage.OnChanged += ScheduleSave;

            Debug.Log($"[SaveService] Loaded. Version: {_data.Version}, Path: {_storage.SavePath}");
        }

        public void LoadFor(ISaveable saveable) => saveable.Load(_data);

        public void Save()
        {
            _savePending = false;
            _saveDelayRemaining = 0f;
            _data = new SaveData();
            
            foreach (var saveable in _saveables)
                saveable.Contribute(_data);
            
            _storage.Write(_data);
        }

        public void Tick(float deltaTime)
        {
            if (!_savePending)
                return;

            _saveDelayRemaining -= deltaTime;
            if (_saveDelayRemaining <= 0f)
                Save();
        }

        public void Reset()
        {
            _savePending = false;
            _saveDelayRemaining = 0f;
            _storage.Delete();

            _data = new SaveData();
            foreach (var saveable in _saveables)
                saveable.Load(_data);

            Debug.Log("[SaveService] Save reset.");
        }

        private void ScheduleSave()
        {
            _savePending = true;
            _saveDelayRemaining = SaveDelay;
        }
    }
}
