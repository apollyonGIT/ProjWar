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
    
    
    public class map
    {
        
        public uint map_id;
        
        public uint site_order;
        
        public float distance_to_next;
        
        public uint site;
        
        public bool exit;
    }
    
    public class maps
    {
        
        private static System.Collections.Generic.Dictionary<string, map> m_records;
        
        public static System.Collections.Generic.Dictionary<string, map> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, map>)Foundations.Excels.ExcelAnalyzer.init("map");
            }
        }
        
        public static bool TryGetValue(string key, out map record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("map", key, out record);
        }
    }
}
