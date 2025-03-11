using Foundations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Console
    {

        //==================================================================================================

        public static void decode_console(Encounter_Dialog_Window window, string raw, string uname)
        {
            if (string.IsNullOrEmpty(raw)) return;

            var commands = raw.Split(';');
            foreach (var _command in commands.Where(t => t != ""))
            {
                var command = Regex.Replace(_command, @"[\s\n]+", "");
                var infos = command.Split("+=");
                var target_info = infos[0];

                var cpn_infos = infos[1].Split(new[] { '(', ')' });
                var cpn_name = cpn_infos[0];
                var cpn_prms = EX_Utility.string_safe_split(cpn_infos[1], ",");

                if (target_info.Contains("btn"))
                {
                    var option_index = int.Parse(target_info["btn".Length..]);
                    var option = window.btn_options[option_index];

                    var cpn_type = Assembly.Load("World").GetType($"World.Encounters.Dialogs.{cpn_name}");
                    var cpn = option.gameObject.AddComponent(cpn_type);
                    
                    var icpn = cpn as IEncounter_Dialog_CPN;
                    icpn.key_name = $"{uname}_{target_info}_{cpn_name}";
                    icpn.@do(option, cpn_prms);
                }
            }
        }

    }
}

