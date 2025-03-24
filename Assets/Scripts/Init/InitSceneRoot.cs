using Foundations;
using Foundations.SceneLoads;
using UnityEngine;

namespace Init
{
    public class InitSceneRoot : SceneRoot<InitSceneRoot>
    {
        public GameObject go_btn_continue_game;

        const string m_next_scene_name = "WorldScene";

        //==================================================================================================

        protected override void on_init()
        {
            base.on_init();
        }


        protected override void on_fini()
        {
            base.on_fini();
        }


        public void btn_start_new_game()
        {
            SceneLoad_Utility.load_scene_with_loading(m_next_scene_name);
        }


        public void btn_continue_game()
        {
            SceneLoad_Utility.load_scene_with_loading(m_next_scene_name);
        }


        public void btn_exit()
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

