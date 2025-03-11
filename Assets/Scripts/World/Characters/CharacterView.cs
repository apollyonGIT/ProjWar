using Addrs;
using Foundations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Characters
{
    public class CharacterView : MonoBehaviour, IPointerClickHandler
    {
        public Character character;
        public GameObject panel;

        public GameObject focus, rest, working;

        public Transform content;

        //工牌模式
        public bool card_mode;
        public void init(Character c)
        {
            character = c;

            Addressable_Utility.try_load_asset(c.desc.portrait, out Sprite s);
            if (s != null)
                panel.GetComponent<Image>().sprite = s;
        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
            cmgr.SelectCharacter(character);
        }
    }
}
