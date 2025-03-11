using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.NewDevice;
using World.Helpers;

namespace World.Devices.DeviceUiViews
{
    public class DeviceStationView : MonoBehaviour,IPointerClickHandler
    {
        public DeviceModule module;

        public Image character_image;

        public void Init(DeviceModule dm)
        {
            module = dm;
        }

        public void SetImage(Sprite s)
        {
            character_image.sprite = s;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (module == null)
                return;

            if(eventData.button == PointerEventData.InputButton.Right)
            {
                Character_Module_Helper.EmptyModule(module);
            }
            else
            {
                Character_Module_Helper.SetModule(module);
            }
        }
    }
}
