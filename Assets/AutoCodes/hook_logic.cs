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
    
    
    public class hook_logic
    {
        
        public uint logic_id;
        
        public int cd;
        
        public float speed_shooting;
        
        public float speed_hooked;
        
        public float speed_reeling_in;
        
        public float elasticity;
        
        public float breaking_overlength;
        
        public int damage_hit;
        
        public int damage_by_period;
        
        public int damage_interval;
    }
    
    public class hook_logics
    {
        
        private static System.Collections.Generic.Dictionary<string, hook_logic> m_records;
        
        public static System.Collections.Generic.Dictionary<string, hook_logic> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, hook_logic>)Foundations.Excels.ExcelAnalyzer.init("hook_logic");
            }
        }
        
        public static bool TryGetValue(string key, out hook_logic record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("hook_logic", key, out record);
        }
    }
}
