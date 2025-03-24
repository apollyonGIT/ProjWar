using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class BasicMeleeUiView : DeviceUiView, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private const float DRAG_SPEED_PER_TICK = 0.95F;

        public Image energy_stick;
        public Image invisible_container;
        public Image rope_line;
        public Slider indicator_energy;

        private float length_current;
        private float length_expected;
        private float length_max;
        private Vector2 stick_dir;

        private NewBasicMelee melee_owner;

        private bool is_dragging;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (invisible_container.rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var position);

            stick_dir = position.normalized;
            length_expected = Mathf.Min(position.magnitude, length_max);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            is_dragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            energy_stick.rectTransform.anchoredPosition = Vector3.zero;
            length_current = 0;
            is_dragging = false;
            rope_line.color = Color.white;
        }

        public override void init()
        {
            base.init();
            indicator_energy.maxValue = NewBasicMelee.energy_max;
            melee_owner = owner as NewBasicMelee;
            length_max = invisible_container.rectTransform.sizeDelta.x * 0.5f;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if (is_dragging)
            {
                if (length_current < length_expected)
                {
                    length_current += DRAG_SPEED_PER_TICK;
                    melee_owner.energy_manual_add();
                    rope_line.color = Color.green;
                }
                else
                {
                    length_current = length_expected;
                    rope_line.color = Color.white;
                }

                if (length_current >= length_max)
                    rope_line.color = Color.white;

                energy_stick.rectTransform.anchoredPosition = stick_dir * length_current;
            }

            rope_line.rectTransform.sizeDelta = new Vector2(length_current, rope_line.rectTransform.sizeDelta.y);
            rope_line.rectTransform.localRotation = Quaternion.FromToRotation(Vector3.right, stick_dir);

            indicator_energy.value = melee_owner.energy_current;
        }
    }
}
