using UnityEngine;
using UnityEngine.EventSystems;
using World.Enemys;

namespace World.Devices.DeviceUiViews
{
    public class UiAim : MonoBehaviour, IDragHandler,IBeginDragHandler, IEndDragHandler,IPointerClickHandler
    {
        public bool is_dragging = false;
        public UiAimPanel aimPanel;

        private ITarget search_target;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            is_dragging = true;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(aimPanel.GetComponent<RectTransform>(),eventData.position,eventData.pressEventCamera,out var position);
            GetComponent<RectTransform>().anchoredPosition = position;

            //RectTransformUtility.ScreenPointToWorldPointInRectangle(aimPanel.GetComponent<RectTransform>(),eventData.position,eventData.pressEventCamera,out var wp);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(aimPanel.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var wp);

            var v2 = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10 - WorldSceneRoot.instance.mainCamera.transform.position.z));
            

            var st = BattleUtility.select_target_in_circle(v2, 1, WorldEnum.Faction.player);
            if (st!= null && st != search_target)
            {
                (search_target as Enemy)?.PreSelect(false);
                search_target = st;
                (search_target as Enemy)?.PreSelect(true);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            is_dragging = false;
            transform.localPosition = Vector3.zero;

            if (search_target != null)
            {
                aimPanel.SetTarget(search_target);

                (search_target as Enemy)?.PreSelect(false);
                (search_target as Enemy)?.Select(true);
                search_target = null;
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            
        }

    }
}
