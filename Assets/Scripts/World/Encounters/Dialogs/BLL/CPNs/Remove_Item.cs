using Commons;
using Foundations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.BackPack;
using World.Loots;

namespace World.Encounters.Dialogs
{
    public class Remove_Item : MonoBehaviour, IEncounter_Dialog_CPN
    {
        List<uint> m_item_id_list = new()
        {
            6000u,
            //6001u,
            //6010u,
            //6011u,
            //6020u,
            //6021u,
            //6022u,
        };

        uint m_current_item_id;

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        BackPackMgr mgr;

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

            Mission.instance.try_get_mgr("BackPack", out mgr);

            var remove_item_id = uint.Parse(args[1]);
            owner.btn_option.interactable = try_get_remove_item(remove_item_id, out var remove_loot);

            owner.btn_click_ac_list.Add(btn_on_click);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                mgr.RemoveLoot(remove_loot);
                Debug.Log($"失去物品{loot_name}");
            }
            #endregion
        }


        bool try_get_remove_item(uint remove_item_id, out Loot remove_loot)
        {
            remove_loot = null;

            foreach (var e in mgr.slots.Where(t => t.loot != null && t.loot.desc.id == remove_item_id))
            {
                remove_loot = e.loot;
                return true;
            }

            foreach (var e in mgr.ow_slots.Where(t => t.loot != null && t.loot.desc.id == remove_item_id))
            {
                remove_loot = e.loot;
                return true;
            }

            return false;
        }
    }
}

