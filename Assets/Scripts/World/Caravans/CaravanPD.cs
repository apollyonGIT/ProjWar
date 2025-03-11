using Addrs;
using Commons;
using Foundations;

namespace World.Caravans
{
    public class CaravanPD : Producer
    {
        public override IMgr imgr => mgr;
        CaravanMgr mgr;

        public CaravanUiView cuv;

        uint caravan_id => Config.current.caravan_id;
        uint caravan_rank = 0u;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new("CaravanMgr", priority);

            var _cell = cell();
            mgr.cell = _cell;

            Addressable_Utility.try_load_asset(_cell._desc.view, out CaravanView model);
            var view = Instantiate(model, transform);
            _cell.add_view(view);

            _cell.add_view(cuv);
        }


        public override void call()
        {
        }


        public Caravan cell()
        {
            return new($"{caravan_id},{caravan_rank}");
        }
    }
}