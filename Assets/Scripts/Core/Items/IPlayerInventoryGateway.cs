using System.Collections.Generic;

namespace Core.Items
{
    public interface IPlayerInventoryGateway
    {
        LootBatch Grant(IReadOnlyList<ItemDefinition> definitions);
    }
}
