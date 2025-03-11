using Addrs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Relic
{
    public class RelicView :MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public Relic data;
        public Image icon;

        public GameObject description_obj;
        public TextMeshProUGUI description;
        public void Init(Relic r)
        {
            data = r;
            if (data.desc.portrait != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(data.desc.portrait, out var s);
                icon.sprite = s;
            }

            description.text = data.desc.description;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            description_obj.gameObject.SetActive(true);
            description.text = data.desc.description;

            description_obj.transform.position = transform.position;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            description_obj.gameObject.SetActive(false);
            description.text = data.desc.description;
        }
    }
}
