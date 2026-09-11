using System.Collections.Generic;

namespace Core.Items
{
    public interface IPlayerInventoryGateway
    {
        LootBatch Grant(IReadOnlyList<ItemDefinition> definitions);
        bool CanUseReward(LootReward reward);
        bool TryUseReward(LootReward reward);
    }
}
