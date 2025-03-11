using Commons;
using Foundations;

namespace World.Progresss
{
    public class ProgressPD : Producer
    {
        public override IMgr imgr => mgr;
        public ProgressUiView puv;
        public ProgressView pv;

        ProgressMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new(Config.MapMgr_Name, priority);
            mgr.Init(WorldContext.instance.r_scene.scene_plot);
            mgr.progress.add_view(pv);
            mgr.progress.add_view(puv);
        }


        public override void call()
        {
        }
    }
}