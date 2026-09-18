using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;
using UnityEngine;

namespace Core.Items
{
    [Serializable]
    public sealed class PreparedConsumableState
    {
        public string ItemDefinitionId;
        public float Value;
    }

    public readonly struct ActiveConsumableEffect
    {
        public string EffectId { get; }
        public float Value { get; }
        public string SourceItemDefinitionId { get; }
        public bool IsActive { get; }

        public ActiveConsumableEffect(string effectId, float value, string sourceItemDefinitionId, bool isActive = false)
        {
            EffectId = effectId;
            Value = value;
            SourceItemDefinitionId = sourceItemDefinitionId;
            IsActive = isActive;
        }

        public ActiveConsumableEffect Activate() => new(EffectId, Value, SourceItemDefinitionId, true);
    }

    public enum ConsumableUseResult { Success, MissingItem, UnsupportedEffect, FamilyOccupied }

    public sealed class RunConsumableService : ISaveable
    {
        public const string Wealth = "creature_gold_bonus";
        public const string Rage = "player_attack_damage_bonus";
        public const string Concentration = "special_attack_cooldown_reduction";

        private readonly PlayerItemStorage _storage;
        private readonly ItemCatalog _catalog;
        private readonly List<ActiveConsumableEffect> _effects = new();
        private readonly IReadOnlyList<ActiveConsumableEffect> _readOnlyEffects;

        public event Action OnEffectsChanged;
        public IReadOnlyList<ActiveConsumableEffect> Effects => _readOnlyEffects;
        public float CreatureGoldMultiplier => 1f + GetEffectTotal(Wealth);
        public float AttackDamageMultiplier => 1f + GetEffectTotal(Rage);
        public float SpecialCooldownMultiplier => 1f - GetEffectTotal(Concentration);

        public RunConsumableService(PlayerItemStorage storage, ItemCatalog catalog)
        {
            _storage = storage;
            _catalog = catalog;
            _readOnlyEffects = _effects.AsReadOnly();
        }

        public ConsumableUseResult CanUse(string instanceId)
        {
            if (!_storage.TryGet(instanceId, out var item) || !_catalog.TryGet(item.ItemDefinitionId, out var definition))
                return ConsumableUseResult.MissingItem;
            if (!IsSupported(definition))
                return ConsumableUseResult.UnsupportedEffect;
            return _effects.Any(effect => effect.EffectId == definition.effectId)
                ? ConsumableUseResult.FamilyOccupied : ConsumableUseResult.Success;
        }

        public bool TryUse(string instanceId) => TryUse(instanceId, out _);

        public bool TryUse(string instanceId, out ConsumableUseResult result)
        {
            result = CanUse(instanceId);
            if (result != ConsumableUseResult.Success)
                return false;

            var definition = _catalog.Get(_storage.Get(instanceId).ItemDefinitionId);
            // Store the effect before inventory callbacks can schedule/save the changed inventory.
            var effect = new ActiveConsumableEffect(definition.effectId, definition.effectValue, definition.itemId);
            _effects.Add(effect);
            if (!_storage.Consume(instanceId))
            {
                _effects.Remove(effect);
                result = ConsumableUseResult.MissingItem;
                return false;
            }

            // SaveService writes the inventory and prepared effects in the same snapshot.
            OnEffectsChanged?.Invoke();
            return true;
        }

        public void BeginLevel()
        {
            // A level begins when play starts, after the lootbox choice, not when its map is generated.
            _effects.RemoveAll(effect => effect.IsActive);
            for (var i = 0; i < _effects.Count; i++)
                _effects[i] = _effects[i].Activate();
            OnEffectsChanged?.Invoke();
        }

        public void EndLevel()
        {
            if (_effects.RemoveAll(effect => effect.IsActive) > 0)
                OnEffectsChanged?.Invoke();
        }

        public void EndRun()
        {
            if (_effects.Count == 0)
                return;
            _effects.Clear();
            OnEffectsChanged?.Invoke();
        }

        public float GetEffectTotal(string effectId)
        {
            foreach (var effect in _effects)
                if (effect.IsActive && effect.EffectId == effectId)
                    return effect.Value;
            return 0f;
        }

        public static bool IsSupported(ItemDefinition definition)
        {
            if (definition == null || definition.category != ItemCategory.Consumable ||
                float.IsNaN(definition.effectValue) || float.IsInfinity(definition.effectValue) || definition.effectValue < 0f)
                return false;
            return definition.effectId switch
            {
                Wealth or Rage => true,
                Concentration => definition.effectValue < 1f,
                _ => false
            };
        }

        public void Load(SaveData data)
        {
            _effects.Clear();
            foreach (var saved in data.PreparedConsumables ?? new List<PreparedConsumableState>())
            {
                if (saved == null || !_catalog.TryGet(saved.ItemDefinitionId, out var definition) ||
                    !IsSupported(definition) || !IsValidSavedValue(definition.effectId, saved.Value) ||
                    _effects.Any(effect => effect.EffectId == definition.effectId))
                    continue;
                _effects.Add(new ActiveConsumableEffect(definition.effectId, saved.Value, definition.itemId));
            }
        }

        public void Contribute(SaveData data)
        {
            // Runs are not resumed by this game. Only effects awaiting their next level survive a restart.
            data.PreparedConsumables = _effects.Where(effect => !effect.IsActive)
                .Select(effect => new PreparedConsumableState
                {
                    ItemDefinitionId = effect.SourceItemDefinitionId,
                    Value = effect.Value
                }).ToList();
        }

        private static bool IsValidSavedValue(string effectId, float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f &&
            (effectId != Concentration || value < 1f);
    }
}
