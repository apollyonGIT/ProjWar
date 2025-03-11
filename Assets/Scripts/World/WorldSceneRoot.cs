using Commons;
using Commons.Levels;
using Foundations;
using Foundations.SceneLoads;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using World.Helpers;
using static Commons.Levels.Level_Mgr;

namespace World
{
    public class WorldSceneRoot : SceneRoot<WorldSceneRoot>
    {
        public ScriptableRendererFeature sr;

        //==================================================================================================

        protected override void on_init()
        {
            var ticker = GetComponent<Ticker_Mono>();
            ticker.init(Config.PHYSICS_TICK_DELTA_TIME);

            WorldContext._init();
            WorldContext.instance.attach();
            load_data_to_ctx();

            BattleContext.instance.Init();

            Context_Init_Helper.init_diy_context();

            Road_Info_Helper.reset();
            Encounters.Dialogs.Encounter_Dialog._init();

            sr.SetActive(false);

            base.on_init();
        }


        protected override void on_fini()
        {
            WorldContext.instance.detach();

            base.on_fini();
        }


        void load_data_to_ctx()
        {
            var ctx = WorldContext.instance;
            var ds = Share_DS.instance;

            //世界id
            ds.try_get_value(Game_Mgr.key_world_id, out string world_id);
            ctx.world_id = world_id;
            Debug.Log($"========  已加载世界：{ctx.world_id}  ========");

            //世界和关卡信息
            ds.try_get_value(Game_Mgr.key_world_and_level_infos, out Dictionary<string, Struct_world_and_level_info> world_and_level_infos);
            var world_and_level_info = world_and_level_infos[world_id];

            ctx.world_progress = world_and_level_info.world_progress;
            ctx.r_game_world = world_and_level_info.r_game_world;
            ctx.r_level = world_and_level_info.r_level_array[ctx.world_progress - 1];
            Debug.Log($"========  已加载关卡：{ctx.r_level.id}_{ctx.r_level.sub_id}，类型：{(EN_level_type)ctx.world_progress}  ========");

            //场景信息
            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);
            AutoCodes.scenes.TryGetValue($"{ctx.r_level.scene[scene_index]}", out ctx.r_scene);
            Debug.Log($"========  已加载子场景：{ctx.r_scene.id}，序列：{scene_index}/{ctx.r_level.scene.Count - 1}  ========");
        }


        public void btn_back_initScene()
        {
            Game_Mgr.on_exit_world(WorldContext.instance.world_progress);
            back_initScene();
        }


        public void back_initScene()
        {
            SceneLoad_Utility.load_scene_async("InitScene");
        }


        public void goto_testScene()
        {
            SceneLoad_Utility.load_scene_with_loading("TestScene");
        }
    }
}

