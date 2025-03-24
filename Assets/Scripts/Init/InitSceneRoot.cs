﻿using Commons;
using Foundations;
using Foundations.SceneLoads;
using UnityEngine;

namespace Init
{
    public class InitSceneRoot : SceneRoot<InitSceneRoot>
    {
        public GameObject go_btn_continue_game;

        //==================================================================================================

        protected override void on_init()
        {
            Share_DS.instance.try_get_value(Game_Mgr.key_is_continue_game, out bool is_continue_game);
            go_btn_continue_game.SetActive(is_continue_game);

            base.on_init();
        }


        protected override void on_fini()
        {
            base.on_fini();
        }


        public void btn_start_new_game()
        {
            Game_Mgr.on_start_new_game();

            SceneLoad_Utility.load_scene("CampScene");
        }


        public void btn_continue_game()
        {
            Game_Mgr.on_continue_game();

            SceneLoad_Utility.load_scene("CampScene");
        }


        public void btn_exit()
        {
            Game_Mgr.on_exit_game();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

