using System;
using System.Collections.Generic;
using UDND.Core;
using UDND.Inventories;
using UDND.Slots;

namespace UI.Inventory
{
    [Serializable]
    public sealed class AutoMergeSeparableStacksStrategy : StackBasedInventoryStrategyBase
    {
        protected override AdditionalMergeOutcome TryResolveAdditionalMergeCandidate(
            IPlacementGeometry geometry,
            InventoryAcceptanceRequest request,
            BaseSlot targetBaseSlot,
            int maxSize,
            Placement sourcePlacement,
            out PlacementCandidate candidate)
        {
            candidate = default;
            foreach (var placement in geometry.Placements)
            {
                if (placement == null || ReferenceEquals(placement, sourcePlacement) ||
                    placement.Stack == null || !placement.Stack.CanStack(request.ItemAdapter) ||
                    placement.Stack.Count >= maxSize)
                {
                    continue;
                }

                if (TryMergeIntoPlacement(geometry, request, placement, maxSize, out candidate))
                    return AdditionalMergeOutcome.Resolved;
            }

            return AdditionalMergeOutcome.NotApplicable;
        }

        public override int GetAcceptableCount(
            IPlacementGeometry geometry,
            InventoryAcceptanceRequest request)
        {
            var item = request?.ItemAdapter;
            var desiredCount = request?.DesiredCount ?? 0;
            if (geometry == null || item == null || desiredCount <= 0)
                return 0;

            var maxSize = GetMaxStackSize(item, DefaultMaxStackSize, AllowItemStackOverride);
            var totalCapacity = 0;
            HashSet<Placement> seenPlacements = null;

            foreach (var slot in geometry.Slots)
            {
                if (ShouldSkipDuplicatePlacementLocation(slot, ref seenPlacements))
                    continue;

                if (slot.IsEmpty && PassesRules(slot, item, Math.Min(desiredCount, maxSize), request))
                {
                    totalCapacity += maxSize;
                }
                else if (!slot.IsEmpty && slot.Stack.CanStack(item))
                {
                    var canFit = Math.Max(0, maxSize - slot.Stack.Count);
                    if (canFit > 0 && PassesRules(slot, item, Math.Min(desiredCount, canFit), request))
                        totalCapacity += canFit;
                }

                if (totalCapacity >= desiredCount)
                    return desiredCount;
            }

            return Math.Min(totalCapacity, desiredCount);
        }
    }
}
