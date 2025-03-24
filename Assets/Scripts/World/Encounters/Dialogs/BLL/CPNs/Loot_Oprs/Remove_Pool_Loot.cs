﻿using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Remove_Pool_Loot : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs_quick(args, out var replace_strs);

            var pool_loot_id = CPN_Utility.get_cache_str_value(args[1]);

            var loot_num_min = int.Parse(CPN_Utility.get_cache_str_value(args[2]));
            var loot_num_max = int.Parse(CPN_Utility.get_cache_str_value(args[3]));
            var loot_num = Random.Range(loot_num_min, loot_num_max + 1);

            if (!CPN_Utility.try_get_loot_id(pool_loot_id, out var loot_id)) return;

            Remove_Loot.remove_loot(owner, replace_strs, m_key_name, loot_id, loot_num);
        }
    }
}

