using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay.Dungeon;
using Core.Items;
using Core.Save;
using Model;
using Utils;

namespace Core.Classes
{
    public enum ClassPurchaseStatus { Available, ClassLocked, RequirementsMissing, NotEnoughGold, Complete, NotConfigured }

    // Owns menu progression and loadout only. Combat consumes these definitions in iteration 2.
    public sealed class ClassProgressionService : ISaveable
    {
        private readonly ClassCatalog _catalog;
        private readonly DungeonSelectionService _dungeons;
        private readonly Player _player;
        private readonly PlayerItemStorage _items;
        private ClassProgressState _state = new();

        public ClassProgressionService(ClassCatalog catalog, DungeonSelectionService dungeons, Player player, PlayerItemStorage items)
        {
            _catalog = catalog;
            _dungeons = dungeons;
            _player = player;
            _items = items;
        }

        public event Action OnChanged;
        public bool HasChosenFirstClass => _state.FirstClassChosen;
        public bool FirstChoicePending => !HasChosenFirstClass && _dungeons.IsCompleted(_catalog.introductionDungeon);
        public int UnlockedCount => _catalog.classes.Count(IsUnlocked);
        public int RuneKeys => _catalog.runeKey == null ? 0 : _items.Items.Count(i => i.ItemDefinitionId == _catalog.runeKey.itemId);
        public bool RequiresIntroduction(DungeonConfig dungeon) => !HasChosenFirstClass && dungeon != null && dungeon == _catalog.introductionDungeon;
        public bool IsUnlocked(ClassDefinition definition) => definition != null && Entry(definition)?.Unlocked == true;
        public bool CanReset(ClassDefinition definition) => IsUnlocked(definition) && UnlockedCount > 1;
        public BigDouble Refund(ClassDefinition definition) => Entry(definition)?.GoldSpent ?? BigDouble.Zero;
        public int Level(ClassDefinition definition, ClassNodeDefinition node) => Entry(definition)?.Nodes.FirstOrDefault(n => n.NodeId == node.id)?.Level ?? 0;

        public bool ChooseFirst(ClassDefinition definition)
        {
            if (!FirstChoicePending || !IsKnown(definition)) return false;
            EnsureEntry(definition).Unlocked = true;
            _state.FirstClassChosen = true;
            _state.ManualAttackId = _catalog.defaultManualAttack != null ? _catalog.defaultManualAttack.id : null;
            _state.AutomaticAttackId = null;
            _state.SpecialAttackId = null;
            OnChanged?.Invoke();
            return true;
        }

        public bool Unlock(ClassDefinition definition)
        {
            if (!HasChosenFirstClass || !IsKnown(definition) || IsUnlocked(definition) || _catalog.runeKey == null) return false;
            var cost = Math.Max(1, definition.unlockKeyCost);
            var keys = _items.Items.Where(i => i.ItemDefinitionId == _catalog.runeKey.itemId).Take(cost).ToArray();
            if (keys.Length != cost) return false;
            EnsureEntry(definition).Unlocked = true;
            _items.Remove(keys);
            OnChanged?.Invoke();
            return true;
        }

        public bool Reset(ClassDefinition definition)
        {
            if (!CanReset(definition)) return false;
            var entry = Entry(definition);
            var refund = entry.GoldSpent;
            foreach (AttackSlot slot in Enum.GetValues(typeof(AttackSlot)))
                if (_catalog.OwnerOf(Equipped(slot)) == definition) SetSlot(slot, null);
            entry.Unlocked = false;
            entry.Nodes.Clear(); // Includes the key node: its reward can be earned again next cycle.
            entry.GoldSpent = BigDouble.Zero;
            _player.GoldTotal += refund;
            OnChanged?.Invoke();
            return true;
        }

        public ClassPurchaseStatus PurchaseStatus(ClassDefinition definition, ClassNodeDefinition node)
        {
            if (!IsKnown(definition) || node == null || !definition.nodes.Contains(node)) return ClassPurchaseStatus.NotConfigured;
            if (!IsUnlocked(definition)) return ClassPurchaseStatus.ClassLocked;
            var level = Level(definition, node);
            if (level >= node.LevelLimit) return ClassPurchaseStatus.Complete;
            if ((node.requirements ?? Array.Empty<ClassNodeRequirement>()).Any(r => r == null || r.node == null ||
                    !definition.nodes.Contains(r.node) || Level(definition, r.node) < Math.Max(1, r.level)))
                return ClassPurchaseStatus.RequirementsMissing;
            if (!node.TryGetCost(level, out var cost) ||
                (node.reward == ClassNodeReward.UnlockAttack && (node.attack == null || string.IsNullOrEmpty(node.attack.id))) ||
                (node.reward == ClassNodeReward.RuneKey && (_catalog.runeKey == null || node.runeKeyReward < 1)))
                return ClassPurchaseStatus.NotConfigured;
            return _player.GoldTotal >= cost ? ClassPurchaseStatus.Available : ClassPurchaseStatus.NotEnoughGold;
        }

        public bool Purchase(ClassDefinition definition, ClassNodeDefinition node)
        {
            if (PurchaseStatus(definition, node) != ClassPurchaseStatus.Available) return false;
            var entry = EnsureEntry(definition);
            var progress = entry.Nodes.FirstOrDefault(p => p.NodeId == node.id);
            node.TryGetCost(progress?.Level ?? 0, out var cost);
            // Prepare rewards before changing gold/progression, so invalid item configuration cannot lose a purchase.
            var keys = new List<PlayerItemInstanceState>();
            if (node.reward == ClassNodeReward.RuneKey)
                for (var i = 0; i < node.runeKeyReward; i++) keys.Add(_items.Create(_catalog.runeKey));
            if (progress == null) entry.Nodes.Add(progress = new ClassNodeProgress { NodeId = node.id });
            progress.Level++;
            entry.GoldSpent = BigDoubleMath.SanitizeNonNegativeInteger(entry.GoldSpent + cost, BigDouble.Zero);
            _player.GoldTotal -= cost;
            if (keys.Count > 0) _items.Place(keys, -1, 0);
            OnChanged?.Invoke();
            return true;
        }

        public bool IsAttackUnlocked(AttackDefinition attack)
        {
            if (attack == null || !HasChosenFirstClass) return false;
            if (attack == _catalog.defaultManualAttack) return true;
            var owner = _catalog.OwnerOf(attack);
            return IsUnlocked(owner) && owner.nodes.Any(n => n != null && n.reward == ClassNodeReward.UnlockAttack &&
                n.attack == attack && Level(owner, n) > 0);
        }

        public IEnumerable<AttackDefinition> AvailableAttacks(AttackSlot slot)
        {
            if (_catalog.defaultManualAttack != null && _catalog.defaultManualAttack.slot == slot && IsAttackUnlocked(_catalog.defaultManualAttack))
                yield return _catalog.defaultManualAttack;
            foreach (var attack in _catalog.classes.Where(IsUnlocked).SelectMany(c => c.nodes)
                         .Where(n => n != null && n.reward == ClassNodeReward.UnlockAttack && n.attack != null)
                         .Select(n => n.attack).Distinct())
                if (attack.slot == slot && IsAttackUnlocked(attack)) yield return attack;
        }

        public AttackDefinition Equipped(AttackSlot slot) => _catalog.FindAttack(slot switch
        {
            AttackSlot.Manual => _state.ManualAttackId,
            AttackSlot.Automatic => _state.AutomaticAttackId,
            AttackSlot.Special => _state.SpecialAttackId,
            _ => null
        });

        public bool Equip(AttackSlot slot, AttackDefinition attack)
        {
            if (!HasChosenFirstClass || !Enum.IsDefined(typeof(AttackSlot), slot) ||
                (attack != null && (attack.slot != slot || !IsAttackUnlocked(attack)))) return false;
            SetSlot(slot, attack?.id);
            OnChanged?.Invoke();
            return true;
        }

        public void Load(SaveData data)
        {
            _state = data.ClassProgressState ?? new ClassProgressState();
            _state.Classes ??= new List<ClassProgressEntry>();
            _state.Classes = _state.Classes.Where(c => c != null && _catalog.FindClass(c.ClassId) != null)
                .GroupBy(c => c.ClassId).Select(g => g.First()).ToList();
            foreach (var entry in _state.Classes)
            {
                entry.Nodes ??= new List<ClassNodeProgress>();
                entry.Nodes = entry.Nodes.Where(n => n != null && n.Level > 0).GroupBy(n => n.NodeId).Select(g => g.First()).ToList();
                entry.GoldSpent = BigDoubleMath.SanitizeNonNegativeInteger(entry.GoldSpent, BigDouble.Zero);
                if (!entry.Unlocked) { entry.Nodes.Clear(); entry.GoldSpent = BigDouble.Zero; }
            }
            if (UnlockedCount > 0) _state.FirstClassChosen = true;
            foreach (AttackSlot slot in Enum.GetValues(typeof(AttackSlot)))
            {
                var attack = Equipped(slot);
                if (attack == null || attack.slot != slot || !IsAttackUnlocked(attack)) SetSlot(slot, null);
            }
        }

        public void Contribute(SaveData data) => data.ClassProgressState = _state;
        private bool IsKnown(ClassDefinition definition) => definition != null && _catalog.classes.Contains(definition);
        private ClassProgressEntry Entry(ClassDefinition definition) => definition == null ? null : _state.Classes.FirstOrDefault(c => c.ClassId == definition.id);
        private ClassProgressEntry EnsureEntry(ClassDefinition definition)
        {
            var entry = Entry(definition);
            if (entry == null) _state.Classes.Add(entry = new ClassProgressEntry { ClassId = definition.id });
            return entry;
        }
        private void SetSlot(AttackSlot slot, string id)
        {
            switch (slot)
            {
                case AttackSlot.Manual: _state.ManualAttackId = id; break;
                case AttackSlot.Automatic: _state.AutomaticAttackId = id; break;
                case AttackSlot.Special: _state.SpecialAttackId = id; break;
            }
        }
    }
}
