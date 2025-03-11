using Commons;
using Foundations;

namespace World.BackPack
{
    public class BackPackPD : Producer
    {
        public BackPackMgrView bmv;
        public override IMgr imgr => mgr;
        BackPackMgr mgr;

        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            mgr = new BackPackMgr("BackPack", priority);
            mgr.add_view(bmv);

            mgr.Init(Config.current.basic_backpack_slot_num);
        }
    }
}
