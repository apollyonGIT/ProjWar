using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.NewDevice;

namespace World.Devices.DeviceUiViews
{
    public class HookTurnTable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Vector2 last_tick_pos;
        public Transform center;
        public BasicHookUiView hookUiView;
        public bool turning;

        public void tick()
        {
            if (turning)
            {
                var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
                Vector2 center_pos = RectTransformUtility.WorldToScreenPoint(WorldSceneRoot.instance.uiCamera, center.transform.position);

                var angle = Vector2.SignedAngle(last_tick_pos - center_pos, current_pos - center_pos);

                if (angle < 0)
                {
                    //transform.Rotate(0, 0, angle * 0.1f);
                    (hookUiView.owner as NewBasicHook).RotateRecycle(angle);
                }
                else
                {
                    (hookUiView.owner as NewBasicHook).rotate_angle += angle;
                    //transform.Rotate(0, 0, angle);
                }
            }
            transform.eulerAngles = new Vector3(0, 0, (hookUiView.owner as NewBasicHook).rotate_angle);
            last_tick_pos = InputController.instance.GetScreenMousePosition();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            InputController.instance.tick_action -= tick;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            InputController.instance.tick_action += tick;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            turning = true;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            turning = false;
        }
    }
}
