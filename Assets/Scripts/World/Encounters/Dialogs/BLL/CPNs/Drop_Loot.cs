using UnityEngine;
using World.Helpers;

namespace World.Encounters.Dialogs
{
    public class Drop_Loot : MonoBehaviour, IEncounter_Dialog_CPN
    {
        Vector2 m_velocity_min = new(0, 0);
        Vector2 m_velocity_max = new(3, 3);

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(Encounter_Dialog_Window_Btn_Option _, string[] args)
        {
            var _event = Encounter_Dialog.instance._event;

            var item_id = uint.Parse(args[0]);
            
            var item_num_min = int.Parse(args[1]);
            var item_num_max = int.Parse(args[2]);
            var item_num = Random.Range(item_num_min, item_num_max + 1);

            var pos = _event.pos + new Vector2(0, 2);

            for (int i = 0; i < item_num; i++)
            {
                Vector2 velocity = new(Random.Range(m_velocity_min.x, m_velocity_max.x + 1), Random.Range(m_velocity_min.y, m_velocity_max.y + 1));
                Drop_Loot_Helper.drop_loot(item_id, pos, velocity);
            }

        }
    }
}

