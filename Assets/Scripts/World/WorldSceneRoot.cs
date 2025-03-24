using Commons;
using Foundations;
using Foundations.Tickers;

namespace World
{
    public class WorldSceneRoot : SceneRoot<WorldSceneRoot>
    {

        //==================================================================================================

        protected override void on_init()
        {
            var ticker = GetComponent<Ticker_Mono>();
            ticker.init(Config.PHYSICS_TICK_DELTA_TIME);

            WorldContext._init();
            WorldContext.instance.attach();
            load_data_to_ctx();

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

            
        }


        //public void btn_back_initScene()
        //{
        //    Game_Mgr.on_exit_world(WorldContext.instance.world_progress);
        //    back_initScene();
        //}


        //public void back_initScene()
        //{
        //    SceneLoad_Utility.load_scene_async("InitScene");
        //}


        //public void goto_testScene()
        //{
        //    SceneLoad_Utility.load_scene_with_loading("TestScene");
        //}
    }
}

