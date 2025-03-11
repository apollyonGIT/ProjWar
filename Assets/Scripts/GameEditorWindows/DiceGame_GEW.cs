using UnityEditor;
using UnityEngine;

namespace GameEditorWindows
{
    public class DiceGame_GEW : EditorWindow
    {
        int m_evil_count;

        //================================================================================================

        [MenuItem("GameEditorWindow/DiceGame")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DiceGame_GEW));
        }


        void OnInspectorUpdate()
        {
            Repaint();
        }


        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("请先运行游戏");
                return;
            }

            GUILayout.BeginVertical();
            {
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("恶魔骰子数量");
                {
                    m_evil_count = EditorGUILayout.IntField(m_evil_count);
                }

                if (GUILayout.Button("开始游戏"))
                {
                    World.DiceGames.Dialogs.DiceGame_Dialog.instance.init("dice_dg"); 
                }

            }
            GUILayout.EndVertical();
        }
    }
}

