using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.BackPack;
using World.Helpers;

namespace World.Encounters.Dialogs
{
    public class Add_Item : MonoBehaviour, IEncounter_Dialog_CPN
    {
        List<uint> m_item_id_list = new()
        { 
            6000u,
            6001u,
            6010u,
            6011u,
            6020u,
            6021u,
            6022u,
        };

        uint m_current_item_id;

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(Encounter_Dialog_Window_Btn_Option owner, string[] args)
        {
            var replace_str = args[0];
            bool is_replace_text = replace_str != "";

            var ed = Encounter_Dialog.instance;
            if (ed.cache_dic.TryGetValue(m_key_name, out var current_item_id))
            {
                m_current_item_id = (uint)current_item_id;
            }
            else
            {
                var index = Random.Range(0, m_item_id_list.Count);
                m_current_item_id = m_item_id_list[index];

                EX_Utility.dic_cover_add(ref ed.cache_dic, m_key_name, m_current_item_id);
            }

            AutoCodes.loots.TryGetValue($"{m_current_item_id}", out var r_loot);
            var loot_name = $"【{Localization_Utility.get_localization(r_loot.name)}】";

            if (is_replace_text)
            {
                var btn_option_name = owner.btn_option_name.text;
                owner.btn_option_name.text = btn_option_name.Replace(replace_str, loot_name);
            }

            owner.btn_click_ac_list.Add(btn_on_click);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                Drop_Loot_Helper.drop_loot(m_current_item_id, WorldContext.instance.caravan_pos + new Vector2(Random.Range(-5, 5), Random.Range(2, 6)), Vector3.zero);

                Debug.Log($"获得物品{loot_name}");
            }
            #endregion
        }
    }
}

