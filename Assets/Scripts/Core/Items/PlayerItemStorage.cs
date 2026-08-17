using System;
using System.Collections.Generic;
using Core.Save;

namespace Core.Items
{
    [Serializable]
    public sealed class PlayerItemInstanceState
    {
        public string InstanceId;
        public string ItemDefinitionId;
        public int Quantity = 1;
    }

    [Serializable]
    public sealed class PlayerItemStorageState
    {
        public List<PlayerItemInstanceState> Items = new();
    }

    public readonly struct LootReward
    {
        public string InstanceId { get; }
        public ItemDefinition Definition { get; }

        public LootReward(string instanceId, ItemDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition;
        }
    }

    public sealed class LootBatch
    {
        public IReadOnlyList<LootReward> Rewards { get; }

        public LootBatch(IReadOnlyList<LootReward> rewards)
        {
            Rewards = rewards;
        }
    }

    public sealed class PlayerItemStorage : ISaveable
    {
        private PlayerItemStorageState _state = new();

        public IReadOnlyList<PlayerItemInstanceState> Items => _state.Items;

        public LootBatch Grant(IReadOnlyList<ItemDefinition> definitions)
        {
            var rewards = new List<LootReward>(definitions.Count);

            foreach (var definition in definitions)
            {
                var instanceId = Guid.NewGuid().ToString("N");
                _state.Items.Add(new PlayerItemInstanceState
                {
                    InstanceId = instanceId,
                    ItemDefinitionId = definition.itemId,
                    Quantity = 1
                });
                rewards.Add(new LootReward(instanceId, definition));
            }

            return new LootBatch(rewards);
        }

        public void Load(SaveData data)
        {
            _state = data.PlayerItemStorageState ?? new PlayerItemStorageState();
            _state.Items ??= new List<PlayerItemInstanceState>();
        }

        public void Contribute(SaveData data)
        {
            data.PlayerItemStorageState = _state;
        }
    }
}
