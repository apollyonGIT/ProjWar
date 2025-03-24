using AutoCodes;
using Commons;
using Foundations;

namespace World.Characters
{
    public class CharacterPD : Producer
    {
        public CharacterMgrView cmv;
        public override IMgr imgr => mgr;
        CharacterMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new(Config.CharacterMgr_Name, priority);
            mgr.add_view(cmv);

            foreach (var id in Config.current.init_roles)
            {
                mgr.AddCharacter((int)id);
            }
        }


        public override void call()
        {
        }
    }
}