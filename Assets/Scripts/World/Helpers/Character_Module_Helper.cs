using Commons;
using Foundations;
using System.Collections.Generic;
using World.Characters;
using World.Devices.NewDevice;

namespace World.Helpers
{
    public class Character_Module_Helper
    {
        public static Dictionary<Character,BasicModule> character_module= new Dictionary<Character, BasicModule>();

        public static Character GetSelectCharacter()
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            return cmgr.select_character;
        }

        public static void SetSelectCharacter(Character c)
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            cmgr.SelectCharacter(c);
        }

        public static void SetModule(BasicModule dm)
        {
            var c = GetSelectCharacter();
            if (c == null)
                return;     //没有选中的角色就不进行任何操作，移除角色通过Empty来进行
            if (character_module.ContainsKey(c))
            {
                EmptyModule(character_module[c]);
            }

            character_module.Add(c, dm);
            set_module(c, dm);
        }

        private static void set_module(Character c , BasicModule dm)
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            dm.SetWorker(true);
            cmgr.SetCharacterWorking(c, true);
            dm.SetModule();
            cmgr.CancelSelectCharacter(c);
        }

        public static void EmptyModule(BasicModule dm)
        {
            var remove_character = new Character();
            var record_module = new BasicModule();

            foreach(var (character,module) in character_module)
            {
                if(module == dm)
                {
                    record_module = module;
                    remove_character = character;
                }
            }

            character_module.Remove(remove_character);
            record_module.SetWorker(false);
            record_module.SetModule();

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            cmgr.SetCharacterWorking(remove_character, false);
        }

        public static Character GetModule(BasicModule dm)
        {
            foreach (var (character, module) in character_module)
            {
                if (module == dm)
                {
                    return character;
                }
            }
            return null;
        }
    }
}
