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
    
    
    public class relic
    {
        
        public uint id;
        
        public string portrait = "";
        
        public string behaviour_script = "";
        
        public string description = "";
        
        public string name = "";
        
        public object diy_obj;
    }
    
    public class relics
    {
        
        private static System.Collections.Generic.Dictionary<string, relic> m_records;
        
        public static System.Collections.Generic.Dictionary<string, relic> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, relic>)Foundations.Excels.ExcelAnalyzer.init("relic");
            }
        }
        
        public static bool TryGetValue(string key, out relic record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("relic", key, out record);
        }
    }
}
