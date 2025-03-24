using UnityEngine;
using World.Enemys;

namespace World.Helpers
{
    public class Enemy_Fini_Helper
    {
        public static void @do(Enemy cell)
        {
            if (cell._desc.loot_list == null) return;

            foreach (var drop_loot in cell._desc.loot_list)
            {
                var prob = (drop_loot.Value + BattleContext.instance.drop_loot_delta) * 100;

                var r = Random.Range(0, 100);

                if (r <= prob)
                {
                    Drop_Loot_Helper.drop_loot(drop_loot.Key, cell.pos, Vector2.zero);
                }
            }
        }
    }
}

