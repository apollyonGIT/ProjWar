//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoCodes
{
    
    
    public class fire_logic
    {
        
        public uint logic_id;
        
        public uint projectile_id;
        
        public int cd;
        
        public System.ValueTuple<float, float> speed;
        
        public System.ValueTuple<int, int> projectile_life_ticks;
        
        public uint salvo;
        
        public float angle;
        
        public string bone_name = "";
        
        public uint repeat;
        
        public uint capacity;
        
        public int reload_by_stage;
        
        public float reload_speed;
        
        public float reload_manual_process;
        
        public System.ValueTuple<float, float> reload_manual_reward;
        
        public bool can_blaze;
        
        public float tick_percent;
        
        public float rapid_fire_tick_percent;
        
        public int damage;
    }
    
    public class fire_logics
    {
        
        private static System.Collections.Generic.Dictionary<string, fire_logic> m_records;
        
        public static System.Collections.Generic.Dictionary<string, fire_logic> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, fire_logic>)Foundations.Excels.ExcelAnalyzer.init("fire_logic");
            }
        }
        
        public static bool TryGetValue(string key, out fire_logic record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("fire_logic", key, out record);
        }
    }
}
