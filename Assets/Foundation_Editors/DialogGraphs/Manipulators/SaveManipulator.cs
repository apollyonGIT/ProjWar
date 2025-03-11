using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class SaveManipulator<T> : MouseManipulator where T : VisualElement
    {
        private bool isSaveKeyPressed = false;
        T owner;

        //==================================================================================================

        public SaveManipulator(T visualElement)
        {
            owner = visualElement;
        }


        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<KeyUpEvent>(OnKeyUp);
        }


        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.UnregisterCallback<KeyUpEvent>(OnKeyUp);
        }


        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.S && e.modifiers == EventModifiers.Control)
            {
                isSaveKeyPressed = true;
                e.PreventDefault();
            }
        }


        private void OnKeyUp(KeyUpEvent e)
        {
            if (e.keyCode == KeyCode.S && isSaveKeyPressed)
            {
                // 执行保存操作
                owner.GetType().GetMethod("save")?.Invoke(owner, null);
                Debug.Log("保存成功");

                isSaveKeyPressed = false;
                e.PreventDefault();
            }
        }
    }
}
