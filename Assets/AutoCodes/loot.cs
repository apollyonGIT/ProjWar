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
    
    
    public class loot
    {
        
        public uint id;
        
        public string name = "";
        
        public string desc = "";
        
        public string view = "";
        
        public object diy_obj;
    }
    
    public class loots
    {
        
        private static System.Collections.Generic.Dictionary<string, loot> m_records;
        
        public static System.Collections.Generic.Dictionary<string, loot> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, loot>)Foundations.Excels.ExcelAnalyzer.init("loot");
            }
        }
        
        public static bool TryGetValue(string key, out loot record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("loot", key, out record);
        }
    }
}
