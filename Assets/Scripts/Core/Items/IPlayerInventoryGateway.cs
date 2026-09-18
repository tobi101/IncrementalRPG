using System.Collections.Generic;

namespace Core.Items
{
    public interface IPlayerInventoryGateway
    {
        LootBatch Grant(IReadOnlyList<LootDrop> drops);
        bool CanUseReward(LootReward reward);
        string GetRewardUseUnavailableKey(LootReward reward);
        bool TryUseReward(LootReward reward);
    }
}
