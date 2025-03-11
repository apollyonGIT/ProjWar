using Foundations;
using UnityEditor;
using UnityEngine;
using World;
using World.Enemys;
using World.Helpers;

namespace GameEditorWindows
{
    public class Loot_GEW : EditorWindow
    {
        int m_id;
        int m_count;

        Vector2 m_velocity_min = new(0, 0);
        Vector2 m_velocity_max = new(3, 3);

        //================================================================================================

        [MenuItem("GameEditorWindow/Loot")]
        public static void ShowWindow()
        {
            GetWindow(typeof(Loot_GEW));
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

            var ctx = WorldContext.instance;

            GUILayout.BeginVertical();
            {
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("");
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("ID");
                    {
                        m_id = EditorGUILayout.IntField(m_id, GUILayout.Width(60));
                    }

                    GUILayout.Label("数量");
                    {
                        m_count = EditorGUILayout.IntField(m_count, GUILayout.Width(60));
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("添加掉落物"))
                {
                    for (int i = 0; i < m_count; i++)
                    {
                        Vector2 velocity = new(Random.Range(m_velocity_min.x, m_velocity_max.x + 1), Random.Range(m_velocity_min.y, m_velocity_max.y + 1));
                        
                        Drop_Loot_Helper.drop_loot((uint)m_id, ctx.caravan_pos + new Vector2(Random.Range(-5,5), Random.Range(2,6)), velocity);
                    }
                }

            }
            GUILayout.EndVertical();
        }
    }
}

