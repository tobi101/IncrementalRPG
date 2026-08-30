using System;
using System.Collections.Generic;
using Core.StateMachine.Features;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Attributes;

namespace Core.Items
{
    public readonly struct ActiveConsumableEffect
    {
        public string EffectId { get; }
        public float Value { get; }
        public string SourceItemDefinitionId { get; }

        public ActiveConsumableEffect(string effectId, float value, string sourceItemDefinitionId)
        {
            EffectId = effectId;
            Value = value;
            SourceItemDefinitionId = sourceItemDefinitionId;
        }
    }

    public sealed class RunConsumableService : IAwakeable
    {
        [Inject] private PlayerItemStorage _storage;
        [Inject] private ItemCatalog _catalog;
        [Inject] private GameplayFeature _gameplay;

        private readonly List<ActiveConsumableEffect> _activeEffects = new();

        public event Action OnEffectsChanged;
        public IReadOnlyList<ActiveConsumableEffect> ActiveEffects => _activeEffects;

        public void OnAwake()
        {
            _gameplay.OnSessionExpired += Clear;
            _gameplay.OnDemoLimitReached += (_, _, _) => Clear();
            _gameplay.OnDisabled += Clear;
        }

        public bool TryUse(string instanceId)
        {
            var item = _storage.Get(instanceId);
            var definition = _catalog.Get(item.ItemDefinitionId);
            if (definition.category != ItemCategory.Consumable)
                return false;

            if (!_storage.Consume(instanceId))
                return false;

            _activeEffects.Add(new ActiveConsumableEffect(
                definition.effectId,
                definition.effectValue,
                definition.itemId));
            OnEffectsChanged?.Invoke();
            return true;
        }

        public float GetEffectTotal(string effectId)
        {
            var total = 0f;
            foreach (var effect in _activeEffects)
            {
                if (effect.EffectId == effectId)
                    total += effect.Value;
            }

            return total;
        }

        private void Clear()
        {
            _activeEffects.Clear();
            OnEffectsChanged?.Invoke();
        }
    }
}
