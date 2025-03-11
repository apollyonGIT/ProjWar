using AutoCodes;
using Foundations;
using UnityEngine;
using World.Loots;

namespace World.Helpers
{
    public class Drop_Loot_Helper
    {
        public static void drop_loot(uint loot_id, Vector2 pos, Vector2 init_velocity)
        {
            loots.TryGetValue(loot_id.ToString(), out var loot);

            Mission.instance.try_get_mgr("Loot", out LootMgr cmgr);
            cmgr.InstantiateLoot(new Loot()
            {
                pos = pos,
                velocity = init_velocity,
                desc = loot,
            });
        }
    }
}
