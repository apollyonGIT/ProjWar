using Foundations;
using System.Collections.Generic;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Add_Random_Buff : MonoBehaviour, IEncounter_Dialog_CPN
    {
        List<string> m_buffs = new()
        { 
            "奥术智慧",
            "灵魂石",
            "铁甲术",
            "奈萨里奥的庇护",
            "熔岩武器",
            "回复",
            "死亡缠绕"
        };

        string m_current_buff;

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(Encounter_Dialog_Window_Btn_Option owner, string[] args)
        {
            var replace_str = args[0];
            bool is_replace_text = replace_str != "";

            var ed = Encounter_Dialog.instance;
            if (ed.cache_dic.TryGetValue(m_key_name, out var current_buff))
            {
                m_current_buff = (string)current_buff;
            }
            else
            {
                var index = Random.Range(0, m_buffs.Count);
                m_current_buff = $"【{m_buffs[index]}】";

                EX_Utility.dic_cover_add(ref ed.cache_dic, m_key_name, m_current_buff);
            }

            if (is_replace_text)
            {
                var btn_option_name = owner.btn_option_name.text;
                owner.btn_option_name.text = btn_option_name.Replace(replace_str, m_current_buff);
            }

            owner.btn_click_ac_list.Add(btn_on_click);
        }


        void btn_on_click()
        {
            Debug.Log($"获得祝福{m_current_buff}");
        }
    }
}

