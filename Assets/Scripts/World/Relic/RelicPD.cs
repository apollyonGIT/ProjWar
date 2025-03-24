using Commons;
using Foundations;

namespace World.Relic
{
    public class RelicPD : Producer
    {
        public override IMgr imgr => mgr;
        RelicMgr mgr;

        public RelicMgrView rv;

        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            mgr = new(Config.RelicMgr_Name,priority);
            mgr.add_view(rv);
        }
    }
}
